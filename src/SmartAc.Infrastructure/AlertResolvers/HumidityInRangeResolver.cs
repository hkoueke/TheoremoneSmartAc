using SmartAc.Application.Extensions;
using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Readings;

namespace SmartAc.Infrastructure.AlertResolvers;

internal sealed class HumidityInRangeResolver : ResolverBase
{
    public override bool IsResolved(DeviceReading reading, AlertType alertType, SensorOptions sensorParams)
    {
        return alertType == AlertType.OutOfRangeHumidity
            && reading.Humidity.InRange(sensorParams.HumidityPctMin, sensorParams.HumidityPctMax)
            || base.IsResolved(reading, alertType, sensorParams);
    }
}