using MediatR;
using SmartAc.Application.Abstractions.Messaging;

namespace SmartAc.Application.Features.DeviceReadings.StoreReadings;

public sealed class StoreReadingCommand : IdempotentCommand<Unit>
{
    public StoreReadingCommand(string deviceSerialNumber, IEnumerable<SensorReading> readings)
    {
        DeviceSerialNumber = deviceSerialNumber;
        Readings = readings;
    }

    public string DeviceSerialNumber { get; }

    public IEnumerable<SensorReading> Readings { get; }
}