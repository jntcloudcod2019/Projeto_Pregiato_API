using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Pregiato.API.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Pregiato.API.Services;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;
using System.Globalization;
using System.Security.Claims;
using Pregiato.API.Services.ServiceModels;
using Pregiato.API.Interfaces;
using Pregiato.API.Response;
using FluentValidation.AspNetCore;
using FluentValidation;
using Npgsql;
using Pregiato.API.System.Text.Json;
using Pregiato.API.System.Text.Json.Serialization;
using Pregiato.API.Validator;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string connectionString = Environment.GetEnvironmentVariable("SECRET_KEY_DATABASE") ?? "Host=191.101.235.250;Port=5432;Database=pregiato;Username=pregiato;Password=pregiato123";
string secretKey = Environment.GetEnvironmentVariable("SECRETKEY_JWT_TOKEN") ?? "3+XcgYxev9TcGXECMBq0ilANarHN68wsDsrhG60icMaACkw9ajU97IYT+cv9IDepqrQjPaj4WUQS3VqOvpmtDw==";
string pathBase = Environment.GetEnvironmentVariable("PATH_BASE");

CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");
Npgsql.TypeMapping.INpgsqlTypeMapper npgsqlTypeMapper = NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();

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


builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
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
builder.Services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IProducersRepository, ProducersRepository>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddSingleton<IEnvironmentVariableProviderEmail, EnvironmentVariableProviderEmail>();


builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<CreateModelRequestValidator>();
builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.Configure<RabbitMQConfig>(options =>
{
    options.RabbitMqUri = Environment.GetEnvironmentVariable("RABBITMQ_URI")
                          ?? builder.Configuration["RabbitMQ:Uri"]
                          ?? "amqps://guest:guest@localhost:5672";

    options.Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int port)
        ? port
        : (builder.Configuration.GetValue<int?>("RabbitMQ:Port") ?? 5672);

    options.QueueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE")
                        ?? "sqs-inboud-sendfile";
});
builder.Services.AddHttpContextAccessor();

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

    OpenApiSecurityScheme securityScheme = new OpenApiSecurityScheme
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

    OpenApiSecurityRequirement securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, ["Bearer"] }
    };
    c.AddSecurityRequirement(securityRequirement);
});


builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    })
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
                    "3+XcgYxev9TcGXECMBq0ilANarHN68wsDsrhG60icMaACkw9ajU97IYT+cv9IDepqrQjPaj4WUQS3VqOvpmtDw=="
                )
            ),

            RoleClaimType = ClaimTypes.Role
        };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PolicyProducers", policy =>
        policy.RequireRole("PRODUCERS"));

    options.AddPolicy("AdminOrManager", policy =>
        policy.RequireRole("ADMINISTRATOR", "MANAGER"));

    options.AddPolicy("AdminOrManagerOrModel", policy =>
        policy.RequireRole("ADMINISTRATOR", "MANAGER", "MODEL"));

    options.AddPolicy("GlobalPolitics", policy =>
        policy.RequireRole("ADMINISTRATOR", "MANAGER", "TELEMARKETING", "PRODUCERS", "COORDINATION", "CEO", "PRODUCTION"));

    options.AddPolicy("GlobalPoliticsAgency", policy =>
        policy.RequireRole("ADMINISTRATOR", "MANAGER", "TELEMARKETING", "Producers", "Coordination", "CEO", "PRODUCTION", "MODEL"));

    options.AddPolicy("ManagementPolicyLevel5", policy =>
        policy.RequireRole("ADMINISTRATOR", "MANAGER", "PRODUCERS", "COORDINATION", "CEO"));

    options.AddPolicy("ManagementPolicyLevel3", policy =>
        policy.RequireRole("ADMINISTRATOR", "MANAGER", "CEO"));

    options.AddPolicy("ManagementPolicyLevel2", policy =>
        policy.RequireRole("ADMINISTRATOR", "CEO"));
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", builder =>
    {
        builder
            .AllowAnyOrigin()
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
        options.JsonSerializerOptions.PropertyNamingPolicy = new UpperCaseNamingPolicy();
        
    });


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

WebApplication app = builder.Build();

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

app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Model Agency API v1");
    c.RoutePrefix = "swagger";
});
app.UseCors("DevPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();