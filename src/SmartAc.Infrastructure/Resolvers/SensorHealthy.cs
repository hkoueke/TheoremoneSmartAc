using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Infrastructure.Resolvers;

internal sealed class SensorHealthy : Resolver
{
    public override bool IsResolved(DeviceReading reading, AlertType alertType, SensorParams sensorParams) 
        => alertType == AlertType.PoorHealth && reading.Health == DeviceHealth.Ok || 
           base.IsResolved(reading, alertType, sensorParams);
}