using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Readings;
using SmartAc.Infrastructure.Alerts.Abstractions;

namespace SmartAc.Infrastructure.Alerts.Resolvers.Sets;

internal sealed class MonoxideLevelSafeResolver : ResolverBase
{
    public override bool IsResolved(in DeviceReading reading, AlertType alertType, SensorOptions sensorParams)
    {
        return alertType == AlertType.DangerousCoLevel
            && reading.CarbonMonoxide < sensorParams.CarbonMonoxideDangerLevel
            || base.IsResolved(reading, alertType, sensorParams);
    }
}