using SmartAc.Application.Extensions;
using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Readings;
using SmartAc.Infrastructure.Alerts.Abstractions;

namespace SmartAc.Infrastructure.Alerts.Resolvers.Sets;

internal sealed class HumidityInRangeResolver : ResolverBase
{
    public override bool IsResolved(in DeviceReading reading, AlertType alertType, SensorOptions sensorParams)
    {
        return alertType == AlertType.OutOfRangeHumidity
            && reading.Humidity.InRange(sensorParams.HumidityPctMin, sensorParams.HumidityPctMax)
            || base.IsResolved(reading, alertType, sensorParams);
    }
}