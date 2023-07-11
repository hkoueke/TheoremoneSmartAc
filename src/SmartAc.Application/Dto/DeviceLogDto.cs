using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Application.Dto;

internal sealed class DeviceLogDto
{
    public string SerialNumber { get; init; } = null!;

    public IReadOnlyList<Alert> Alerts { get; init; } = null!;

    public IReadOnlyList<DeviceReading> DeviceReadings { get; init; } = null!;
}