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
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("SECRET_KEY_DATABASE") ?? "Host=191.101.235.250;Port=5432;Database=pregiato;Username=pregiato;Password=pregiato123";
var secretKey = Environment.GetEnvironmentVariable("SECRETKEY_JWT_TOKEN") ?? "3+XcgYxev9TcGXECMBq0ilANarHN68wsDsrhG60icMaACkw9ajU97IYT+cv9IDepqrQjPaj4WUQS3VqOvpmtDw==";
var pathBase = Environment.GetEnvironmentVariable("PATH_BASE");

CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

// Configuração do DbContext
builder.Services.AddDbContext<ModelAgencyContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddDbContextFactory<ModelAgencyContext>(
    options =>
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null
            );
        });
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
        options.LogTo(Console.WriteLine, LogLevel.Information);
    },
    ServiceLifetime.Scoped
);

// Configuração de serviços
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

// Injeção de dependências
builder.Services.AddScoped<CustomResponse>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IServiceUtilites, ServiceUtilites>();
builder.Services.AddSingleton<IBrowserService, BrowserService>();
builder.Services.AddScoped<IModelRepository, ModelsRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IClientBillingRepository, ClientBillingRepository>();
builder.Services.AddSingleton<IEnvironmentVariableProviderEmail, EnvironmentVariableProviderEmail>();

// Configurações adicionais
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));

// Configuração do Swagger
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

// Configuração de Autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Environment.GetEnvironmentVariable("ISSUER_JWT") ?? "PregiatoAPI",
            ValidateAudience = true,
            ValidAudience = Environment.GetEnvironmentVariable("AUDIENCE_JWT") ?? "PregiatoAPIToken",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(
                  Environment.GetEnvironmentVariable("SECRETKEY_JWT_TOKEN") ??
                  "3+XcgYxev9TcGXECMBq0ilANarHN68wsDsrhG60icMaACkw9ajU97IYT+cv9IDepqrQjPaj4WUQS3VqOvpmtDw=="))
        };
    });

// Configuração do RabbitMQ
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

// Configuração de Autorização
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrManager", policy =>
        policy.RequireRole("Administrator", "Manager"));

    options.AddPolicy("AdminOrManagerOrModel", policy =>
        policy.RequireRole("Administrator", "Manager", "Model"));
});

// Configuração de CORS
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

// Configuração de Controllers
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

// Configuração de Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

var app = builder.Build();

// Configuração do Pipeline de Requisições
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
    app.Use((context, next) =>
    {
        context.Request.PathBase = new PathString(pathBase);
        return next();
    });
}

// Ordem CORRETA dos middlewares:
app.UseRouting();
app.UseCors("AllowAllOrigins");

// Middlewares de segurança adicionados aqui:
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Model Agency API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();

app.Run();