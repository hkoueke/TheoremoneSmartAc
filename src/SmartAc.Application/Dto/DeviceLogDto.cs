using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Application.Dto;

internal sealed class DeviceLogDto
{
    public string SerialNumber { get; init; } = string.Empty;

    public IReadOnlyList<Alert> Alerts { get; init; } = new List<Alert>();

    public IReadOnlyList<DeviceReading> DeviceReadings { get; init; } = new List<DeviceReading>();
}