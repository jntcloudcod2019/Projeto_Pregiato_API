using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Pregiato.API.Data;
using Microsoft.IdentityModel.Tokens;
using Pregiato.API.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Pregiato.API.Services;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;
using Pregiato.API.Models;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Environment.EnvironmentName = Environments.Production;

Console.WriteLine($" Ambiente Atual: {builder.Environment.EnvironmentName}");

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

builder.Configuration.AddConfiguration(config);

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

var connectionString = config.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("ERRO: A string de conexão com o banco de dados não foi encontrada no appsettings.json!");
}

builder.Services.AddDbContext<ModelAgencyContext>(options =>
    options.UseNpgsql(connectionString)
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
           .LogTo(Console.WriteLine, LogLevel.Information));

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<DigitalSignatureService>();
builder.Services.AddSingleton<SmtpServerService>();
builder.Services.AddHostedService<SmtpServerService>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IModelRepository, ModelsRepository>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IServiceUtilites, ServiceUtilites>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IClientBillingRepository, ClientBillingRepository>();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Model Agency API", Version = "v1" });

    c.MapType<MetodoPagamento>(() => new OpenApiSchema
    {
        Type = "string",
        Example = new OpenApiString("CartaoCredito"),
        Description = "Método de pagamento. Valores permitidos: CartaoCredito, CartaoDebito, Pix, Dinheiro, LinkPagamento"
    });

    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First()); 

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, new[] { "Bearer" } }
    };
    c.AddSecurityRequirement(securityRequirement);
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrManager", policy => 
        policy.RequireRole("Administrator", "Manager"));
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrManagerOrModel", policy =>
        policy.RequireRole("Administrator", "Manager", "Model"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;              
        options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
        options.JsonSerializerOptions.Converters.Add(new DecimalJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new MetodoPagamentoConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter("dd-MM-yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy"));
    });

builder.Services.AddAuthorization();
builder.WebHost.UseUrls("http://+:8080");
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Model Agency API v1");
    c.RoutePrefix = string.Empty; // Deixa o Swagger disponível em http://localhost:8080
});
app.UseCors("AllowAllOrigins");
app.UseRouting(); 
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

