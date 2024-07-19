using SmartAc.Application.Extensions;
using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Readings;

namespace SmartAc.Infrastructure.AlertResolvers;

internal sealed class TempInRangeResolver : ResolverBase
{
    public override bool IsResolved(DeviceReading reading, AlertType alertType, SensorOptions sensorParams)
    {
        return alertType == AlertType.OutOfRangeTemp
            && reading.Temperature.InRange(sensorParams.TemperatureMin, sensorParams.TemperatureMax)
            || base.IsResolved(reading, alertType, sensorParams);
    }
}