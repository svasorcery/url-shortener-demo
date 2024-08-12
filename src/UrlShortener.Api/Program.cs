using FluentValidation;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api;
using UrlShortener.Api.Services;
using UrlShortener.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;


services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContext<ApplicationDbContext>(x =>
    x.UseNpgsql(configuration.GetConnectionString("postgres")));

services.AddValidatorsFromAssembly(
    assembly: Assembly.GetExecutingAssembly(),
    lifetime: ServiceLifetime.Scoped,
    includeInternalTypes: true
    );

services.AddScoped<UrlShorteningService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyDatabaseMigrations();
}

app.MapUrlShortenerEndpoints();

app.UseHttpsRedirection();

app.Run();


// Make the implicit Program class public so test projects can access it
public partial class Program { }