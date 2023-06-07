using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartAc.Domain.DeviceReadings;

public sealed class DeviceReading : EntityBase
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DeviceReadingId { get; private set; }

    public decimal Temperature { get; set; }

    public decimal Humidity { get; set; }

    public decimal CarbonMonoxide { get; set; }

    public DeviceHealth Health { get; set; }

    public DateTimeOffset RecordedDateTime { get; set; }

    public DateTimeOffset ReceivedDateTime { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ProcessedDateTime { get; private set; }

    public string DeviceSerialNumber { get; set; } = string.Empty;

    public void MarkAsProcessed(DateTimeOffset processedDateTimeOffset) => ProcessedDateTime = processedDateTimeOffset;
}