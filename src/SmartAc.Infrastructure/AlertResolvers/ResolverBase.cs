using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Readings;

namespace SmartAc.Infrastructure.AlertResolvers;

internal abstract class ResolverBase
{
    private ResolverBase? _nextResolver;

    public ResolverBase SetNext(ResolverBase nextResolver)
    {
        _nextResolver = nextResolver;
        return nextResolver;
    }

    public virtual bool IsResolved(DeviceReading reading, AlertType alertType, SensorOptions sensorParams)
        => _nextResolver?.IsResolved(reading, alertType, sensorParams) ?? false;
}