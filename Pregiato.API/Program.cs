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

var builder = WebApplication.CreateBuilder(args);

// Definir o ambiente
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
Console.WriteLine($" Ambiente Atual: {environment}");

// Configuração do `appsettings.json`
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

builder.Configuration.AddConfiguration(config);

// Configurar o DbContext
builder.Services.AddDbContext<ModelAgencyContext>(options =>
    options.UseNpgsql(config.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging(environment == "Development")
           .LogTo(Console.WriteLine, LogLevel.Information));

// Configuração de Repositórios e Serviços
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IModelRepository, ModelsRepository>();
builder.Services.AddScoped<IClientBillingRepository, ClientBillingRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<DigitalSignatureService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

// Configuração do Swagger com suporte a JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Model Agency API", Version = "v1" });

    c.MapType<MetodoPagamento>(() => new OpenApiSchema
    {
        Type = "string",
        Example = new OpenApiString("CartaoCredito"), // Exemplo padrão
        Description = "Método de pagamento. Valores permitidos: " +
                     "CartaoCredito, CartaoDebito, Pix, Dinheiro, LinkPagamento"
    });

    // Adicionar suporte a autenticação JWT no Swagger
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

// Configuração de JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new ArgumentNullException("SecretKey", " ERRO: A chave secreta do JWT não foi encontrada no appsettings.json!");
}

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
    options.AddPolicy("AdministratorPolicy", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("ManagerPolicy", policy => policy.RequireRole("Manager"));
    options.AddPolicy("ModelPolicy", policy => policy.RequireRole("Model"));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter("dd-MM-yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy"));
        options.JsonSerializerOptions.Converters.Add(new MetodoPagamentoConverter());
        options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddAuthorization();
builder.WebHost.UseUrls("http://+:" + (Environment.GetEnvironmentVariable("PORT") ?? "8080"));
var app = builder.Build();

// Configurar Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {


        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Model Agency API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz (http://localhost:5000)
        c.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
        {
            ["activated"] = false // Desativa o syntax highlighting para melhorar o desempenho
        };


       
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();