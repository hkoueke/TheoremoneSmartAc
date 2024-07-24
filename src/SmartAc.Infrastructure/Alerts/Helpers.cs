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
        => new AlertProcessor(options).SetNext(new AlertResolver(options));

    public static ResolverBase GetResolver()
    {
        return new TempInRangeResolver()
            .SetNext(new SensorHealthyResolver())
            .SetNext(new MonoxideLevelSafeResolver())
            .SetNext(new MonoxideInRangeResolver())
            .SetNext(new HumidityInRangeResolver());
    }
}
