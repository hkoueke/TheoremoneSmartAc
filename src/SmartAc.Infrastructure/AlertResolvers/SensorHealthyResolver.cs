using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Readings;

namespace SmartAc.Infrastructure.AlertResolvers;

internal sealed class SensorHealthyResolver : ResolverBase
{
    public override bool IsResolved(DeviceReading reading, AlertType alertType, SensorOptions sensorParams) 
        => alertType == AlertType.PoorHealth 
        && reading.Health == DeviceHealth.Ok 
        || base.IsResolved(reading, alertType, sensorParams);
}