using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Pregiato.API.Data;
using Pregiato.API.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 
builder.Services.AddDbContext<ModelAgencyContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// 
builder.Services.AddScoped<IClientRepository, ClientRepository>();
// 
builder.Services.AddControllers();

// 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Model Agency API",
        Version = "v1",
        Description = "API for managing models, clients, contracts, and jobs in a model agency."
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Model Agency API v1");
        c.RoutePrefix = string.Empty; // Swagger será acessado na raiz (http://localhost:5000)
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Ensure database creation (opcional, para ambiente de desenvolvimento)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ModelAgencyContext>();
    dbContext.Database.EnsureCreated();
}
