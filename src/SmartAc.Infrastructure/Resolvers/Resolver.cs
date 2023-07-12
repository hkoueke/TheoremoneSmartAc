using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Infrastructure.Resolvers;

internal class Resolver
{
    private Resolver? _nextResolver;

    public Resolver SetNext(Resolver nextResolver)
    {
        _nextResolver = nextResolver;
        return nextResolver;
    }

    public virtual bool IsResolved(DeviceReading reading, AlertType alertType, SensorParams sensorParams)
        => _nextResolver?.IsResolved(reading, alertType, sensorParams) ?? false;
}