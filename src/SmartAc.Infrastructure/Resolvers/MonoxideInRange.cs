using SmartAc.Application.Extensions;
using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Infrastructure.Resolvers;

internal sealed class MonoxideInRange : Resolver
{
    public override bool IsResolved(DeviceReading reading, AlertType alertType, SensorParams sensorParams)
    {
        return alertType == AlertType.OutOfRangeCo && 
            reading.CarbonMonoxide.InRange(sensorParams.CarbonMonoxidePpmMin, sensorParams.CarbonMonoxidePpmMax) ||
            base.IsResolved(reading, alertType, sensorParams);
    }
}