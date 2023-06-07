using SmartAc.API;
using System.Text.Json.Serialization;
using SmartAc.API.Middlewares;
using SmartAc.Application.Extensions;
using SmartAc.Infrastructure.Extensions;
using SmartAc.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartAc.API.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication(builder.Configuration)
                .AddPersistence(builder.Configuration)
                .AddInfrastructure(builder.Configuration);

builder.Services.TryAddTransient<ExceptionHandlingMiddleware>();

builder.Services.TryAddScoped<IAuthorizationHandler, ValidTokenAuthorizationHandler>();

builder.Services
    //.ConfigureApiBehaviorOptions(options =>
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOptions(builder.Configuration);

builder.Services.AddOpenApiDocumentation();

builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.EnsureDatabaseSetup();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseOpenApiDocumentation();

app.MapSmartAcControllers();

app.Run();
