using Microsoft.Extensions.Options;
using SmartAc.Application.Options;
using SmartAc.Infrastructure.Alerts.Abstractions;
using SmartAc.Infrastructure.Alerts.Processors;
using SmartAc.Infrastructure.Alerts.Resolvers;
using SmartAc.Infrastructure.Alerts.Resolvers.Sets;

namespace SmartAc.Infrastructure.Alerts;

internal static class Helpers
{
    public static ProcessorBase GetProcessor(IOptionsMonitor<SensorOptions> options)
    {
        var processor = new AlertProcessor(options);
        processor.SetNext(new AlertResolver(options));

        return processor;
    }

    public static ResolverBase GetResolver()
    {
        var resolver = new TempInRangeResolver();
        resolver
            .SetNext(new SensorHealthyResolver())
            .SetNext(new MonoxideLevelSafeResolver())
            .SetNext(new MonoxideInRangeResolver())
            .SetNext(new HumidityInRangeResolver());

        return resolver;
    }
}
