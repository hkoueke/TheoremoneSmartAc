using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Readings;
using SmartAc.Infrastructure.Alerts.Abstractions;

namespace SmartAc.Infrastructure.Alerts.Resolvers.Sets;

internal sealed class SensorHealthyResolver : ResolverBase
{
    public override bool IsResolved(in DeviceReading reading, AlertType alertType, SensorOptions sensorParams)
        => alertType == AlertType.PoorHealth
        && reading.Health == DeviceHealth.Ok
        || base.IsResolved(reading, alertType, sensorParams);
}