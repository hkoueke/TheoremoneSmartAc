using SmartAc.Application.Extensions;
using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Infrastructure.AlertResolvers;

internal sealed class TempInRange : Resolver
{
    public override bool IsResolved(DeviceReading reading, AlertType alertType, SensorParams sensorParams)
        => alertType == AlertType.OutOfRangeTemp &&
           reading.Temperature.InRange(sensorParams.TemperatureMin, sensorParams.TemperatureMax) ||
           base.IsResolved(reading, alertType, sensorParams);
}