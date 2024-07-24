using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Readings;

namespace SmartAc.Infrastructure.Alerts.Abstractions;

internal abstract class ResolverBase
{
    private ResolverBase? _nextResolver;

    public ResolverBase SetNext(ResolverBase nextResolver)
    {
        _nextResolver = nextResolver;
        return nextResolver;
    }

    public virtual bool IsResolved(in DeviceReading reading, AlertType alertType, SensorOptions sensorOptions)
        => _nextResolver?.IsResolved(reading, alertType, sensorOptions) ?? default;
}