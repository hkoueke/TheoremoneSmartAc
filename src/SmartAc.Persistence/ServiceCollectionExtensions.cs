using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Persistence.Repositories;

namespace SmartAc.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("smartac") ?? "Data Source=SmartAc.db";

        services.AddDbContext<SmartAcContext>(options =>
        {
            options.UseSqlite(connectionString);
                //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);

            options.LogTo(
                Console.WriteLine, 
                new[] { RelationalEventId.CommandExecuted }, 
                LogLevel.Information);
        });

        services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.TryAddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}