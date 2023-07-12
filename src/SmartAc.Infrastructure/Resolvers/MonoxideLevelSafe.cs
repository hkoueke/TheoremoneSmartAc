using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Infrastructure.Resolvers;

internal sealed class MonoxideLevelSafe : Resolver
{
    public override bool IsResolved(DeviceReading reading, AlertType alertType, SensorParams sensorParams)
    {
        return alertType == AlertType.DangerousCoLevel && 
               reading.CarbonMonoxide < sensorParams.CarbonMonoxideDangerLevel ||
               base.IsResolved(reading, alertType, sensorParams);
    }
}