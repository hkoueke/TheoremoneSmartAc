using SmartAc.Application.Extensions;
using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Infrastructure.BackgroundJobs;

public static class DeviceReadingExtensions
{
    public static IEnumerable<Alert> GetAlerts(this DeviceReading reading, SensorParams sensorParams)
    {
        if (!reading.Temperature.InRange(sensorParams.TemperatureMin, sensorParams.TemperatureMax))
        {
            yield return Alert.CreateNew(
                AlertType.OutOfRangeTemp,
                $"Sensor {reading.DeviceSerialNumber} reported out-of-range Temperature", reading.DeviceSerialNumber, reading.RecordedDateTime);
        }

        if (!reading.CarbonMonoxide.InRange(sensorParams.CarbonMonoxidePpmMin, sensorParams.CarbonMonoxidePpmMax))
        {
            yield return Alert.CreateNew(
                AlertType.OutOfRangeCo,
                $"Sensor {reading.DeviceSerialNumber} reported out-of-range carbon Monoxide levels", reading.DeviceSerialNumber, reading.RecordedDateTime);
        }

        if (reading.CarbonMonoxide >= sensorParams.CarbonMonoxideDangerLevel)
        {
            yield return Alert.CreateNew(
                AlertType.DangerousCoLevel,
                $"Sensor {reading.DeviceSerialNumber} - Reported CO value has exceeded danger limit", reading.DeviceSerialNumber, reading.RecordedDateTime);
        }

        if (!reading.Humidity.InRange(sensorParams.HumidityPctMin, sensorParams.HumidityPctMax))
        {
            yield return Alert.CreateNew(
                AlertType.OutOfRangeHumidity,
                $"Sensor {reading.DeviceSerialNumber} reported out-of-range humidity levels", reading.DeviceSerialNumber, reading.RecordedDateTime);
        }

        if (reading.Health != DeviceHealth.Ok)
        {
            yield return Alert.CreateNew(
                AlertType.PoorHealth,
                $"Sensor {reading.DeviceSerialNumber} is reporting health problem: {reading.Health}", reading.DeviceSerialNumber, reading.RecordedDateTime);
        }
    }
}