using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Readings;

namespace SmartAc.Infrastructure.AlertResolvers;

internal sealed class MonoxideLevelSafeResolver : ResolverBase
{
    public override bool IsResolved(DeviceReading reading, AlertType alertType, SensorOptions sensorParams)
    {
        return alertType == AlertType.DangerousCoLevel
            && reading.CarbonMonoxide < sensorParams.CarbonMonoxideDangerLevel
            || base.IsResolved(reading, alertType, sensorParams);
    }
}