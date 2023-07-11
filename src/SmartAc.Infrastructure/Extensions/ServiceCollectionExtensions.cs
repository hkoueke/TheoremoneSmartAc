using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using SmartAc.Application.Abstractions.Services;
using SmartAc.Infrastructure.BackgroundJobs;
using SmartAc.Infrastructure.Options;
using SmartAc.Infrastructure.Services;

namespace SmartAc.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterBackgroundServices(services, configuration);
        RegisterJwtService(services);
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
        services.AddQuartz(configure =>
        {
            var jobKey = new JobKey(nameof(ProcessNewReadingsJob));

            var intervalInSeconds =
                configuration
                    .GetRequiredSection("BackgroundJobParams")
                    .GetValue<int>("IntervalInSeconds");

            configure
                .AddJob<ProcessNewReadingsJob>(jobKey)
                .AddTrigger(trigger =>
                    trigger.ForJob(jobKey)
                        .WithSimpleSchedule(schedule =>
                            schedule.WithIntervalInSeconds(intervalInSeconds)
                                .RepeatForever()));

            configure.UseMicrosoftDependencyInjectionJobFactory();
        });

        services.AddQuartzHostedService();
    }

    private static void RegisterJwtService(IServiceCollection services)
    {
        services.TryAddTransient<ISmartAcJwtService, SmartAcJwtService>();
    }
}