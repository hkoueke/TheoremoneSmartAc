using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using SmartAc.Application.Abstractions.Services;
using SmartAc.Infrastructure.Options;
using SmartAc.Infrastructure.Services;

namespace SmartAc.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterServices(services, configuration);
        RegisterBackgroundServices(services, configuration);
        RegisterOptions(services, configuration);

        return services;
    }

    private static void RegisterOptions(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<BackgroundJobParams>()
            .BindConfiguration("BackgroundJobParams")
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void RegisterBackgroundServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(configure => configure.UseMicrosoftDependencyInjectionJobFactory());
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
        services.ConfigureOptions<ReadingProcessingJobSetup>();
    }

    private static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddTransient<ISmartAcJwtService, SmartAcJwtService>();
    }
}