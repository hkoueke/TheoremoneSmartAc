using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using SmartAc.Application.Abstractions.Services;
using SmartAc.Infrastructure.BackgroundJobs;
using SmartAc.Infrastructure.BackgroundJobs.Handlers;
using SmartAc.Infrastructure.Services;

namespace SmartAc.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddTransient<ISmartAcJwtService, SmartAcJwtService>();
        services.TryAddScoped<AlertProducerHandler>();
        services.TryAddScoped<AlertResolverHandler>();

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

        return services;
    }
}