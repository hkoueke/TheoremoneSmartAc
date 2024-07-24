using SmartAc.Application.Extensions;
using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Readings;
using SmartAc.Infrastructure.Alerts.Abstractions;

namespace SmartAc.Infrastructure.Alerts.Resolvers.Sets;

internal sealed class TempInRangeResolver : ResolverBase
{
    public override bool IsResolved(in DeviceReading reading, AlertType alertType, SensorOptions sensorParams)
    {
        return alertType == AlertType.OutOfRangeTemp
            && reading.Temperature.InRange(sensorParams.TemperatureMin, sensorParams.TemperatureMax)
            || base.IsResolved(reading, alertType, sensorParams);
    }
}