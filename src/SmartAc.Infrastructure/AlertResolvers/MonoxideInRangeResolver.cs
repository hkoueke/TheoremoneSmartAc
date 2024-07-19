using SmartAc.Application.Extensions;
using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Readings;

namespace SmartAc.Infrastructure.AlertResolvers;

internal sealed class MonoxideInRangeResolver : ResolverBase
{
    public override bool IsResolved(DeviceReading reading, AlertType alertType, SensorOptions sensorParams)
    {
        return alertType == AlertType.OutOfRangeCo
            && reading.CarbonMonoxide.InRange(sensorParams.CarbonMonoxidePpmMin, sensorParams.CarbonMonoxidePpmMax)
            || base.IsResolved(reading, alertType, sensorParams);
            
    }
}