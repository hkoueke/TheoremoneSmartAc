using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartAc.Application.PipelineBehaviors;

namespace SmartAc.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterFluentValidation(services);
        RegisterMediatR(services);
        RegisterAutoMapper(services);
        return services;
    }

    private static void RegisterFluentValidation(IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AssemblyReference>(includeInternalTypes: true);
    }

    private static void RegisterMediatR(IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<AssemblyReference>();
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(IdempotencyBehavior<,>));
            config.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });
    }

    private static void RegisterAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(AssemblyReference));
    }

    private static void RegisterOptions(IServiceCollection services)
    {

    }
}