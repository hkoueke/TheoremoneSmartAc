using SmartAc.Application.Extensions;
using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Infrastructure.AlertResolvers;

internal sealed class HumidityInRange : Resolver
{
    public override bool IsResolved(DeviceReading reading, AlertType alertType, SensorParams sensorParams)
        => alertType == AlertType.OutOfRangeHumidity && 
           reading.Humidity.InRange(sensorParams.HumidityPctMin, sensorParams.HumidityPctMax) || 
           base.IsResolved(reading, alertType, sensorParams);
}