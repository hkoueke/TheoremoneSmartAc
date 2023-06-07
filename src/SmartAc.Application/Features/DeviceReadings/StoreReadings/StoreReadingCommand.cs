using MediatR;

namespace SmartAc.Application.Features.DeviceReadings.StoreReadings;

public sealed record StoreReadingCommand(string DeviceSerialNumber, IEnumerable<SensorReading> Readings) : IRequest;