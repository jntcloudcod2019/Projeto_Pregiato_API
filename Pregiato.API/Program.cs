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
using Pregiato.API.Services.ServiceModels;
using Pregiato.API.Interfaces;
using Pregiato.API.Response;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("SECRET_KEY_DATABASE");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("A variável de ambiente 'SECRET_KEY_DATABASE' não foi definida!");

// 1) Registro de DbContext normal (padrão: scoped)
builder.Services.AddDbContext<ModelAgencyContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// 2) Registro de DbContextFactory como *scoped*, para evitar o conflito
builder.Services.AddDbContextFactory<ModelAgencyContext>(
    options =>
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null
            );
        });
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
        options.LogTo(Console.WriteLine, LogLevel.Information);
    },
    ServiceLifetime.Scoped
);

CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

// Exemplo de config RabbitMQ
builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));

// Lê PATH_BASE do ambiente, se houver
var pathBase = Environment.GetEnvironmentVariable("PATH_BASE");

// HttpContext e Endpoint
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

// Automapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Repositórios e Services
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IModelRepository, ModelsRepository>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddSingleton<IEnvironmentVariableProviderEmail, EnvironmentVariableProviderEmail>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IServiceUtilites, ServiceUtilites>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IClientBillingRepository, ClientBillingRepository>();
builder.Services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
builder.Services.AddScoped<CustomResponse>();
builder.Services.AddSingleton<IBrowserService, BrowserService>();

// Swagger
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

// JWT
var secretKey = Environment.GetEnvironmentVariable("SECRETKEY_JWT_TOKEN");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("ISSUER_JWT"),
            ValidAudience = Environment.GetEnvironmentVariable("AUDIENCE_JWT"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// RabbitMQ Config
builder.Services.Configure<RabbitMQConfig>(options =>
{
    options.RabbitMqUri = Environment.GetEnvironmentVariable("RABBITMQ_URI")
        ?? builder.Configuration["RabbitMQ:Uri"]
        ?? "amqps://guest:guest@localhost:5672";

    options.Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var port)
        ? port
        : (builder.Configuration.GetValue<int?>("RabbitMQ:Port") ?? 5672);

    options.QueueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE")
       ?? "sqs-inboud-sendfile";
});

// Regras de Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrManager", policy =>
        policy.RequireRole("Administrator", "Manager"));

    options.AddPolicy("AdminOrManagerOrModel", policy =>
        policy.RequireRole("Administrator", "Manager", "Model"));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader();
        });
});

// MVC / Controllers
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

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

// Constrói o app
var app = builder.Build();

// Usa path base, se houver
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
    app.Use((context, next) =>
    {
        context.Request.PathBase = new PathString(pathBase);
        return next();
    });
}

/// Swagger ? agora estará em /swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Model Agency API v1");
    c.RoutePrefix = "swagger";
});

// Se quiser que o Swagger apareça diretamente na raiz ("/"),
// é só trocar c.RoutePrefix = string.Empty;

 app.UseDefaultFiles();
 app.UseStaticFiles();

app.UseCors("AllowAllOrigins");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();