
using Microsoft.Extensions.Configuration;
using Npgsql;
using SanuApi.Aplication.Services;
using SanuApi.Application.Interfaces;
using SanuApi.Application.Services;
using SanuApi.Domain.Interfaces;
using SanuApi.Infrastructure.Repositories;
using System.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 🚀 Conexión PostgreSQL

builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connStr))
{
    Console.WriteLine("⚠️ Connection string es null o vacío!");
}
else
{
    Console.WriteLine($"Connection string: {connStr}");
}

// 🚀 Repositorios
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();
builder.Services.AddScoped<ICustomerMembershipRepository, CustomerMemberShipRepository>();
builder.Services.AddScoped<IHealthCustomerRepository, HealthCustomerRepository>(); 

// 🚀 Servicios
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();

// 🚀 Controladores + JSON con enums como string
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// 🚀 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🚀 Configurar CORS para permitir Swagger
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// 🚀 Middleware CORS
app.UseCors("AllowAll");

// 🚀 Middleware Swagger activo siempre
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sanu API v1");
    c.RoutePrefix = string.Empty; // 👈 esto deja Swagger en la raíz "/"
});

// 🚀 Redirigir "/" a Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));


// 🚀 Middlewares estándar
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
