using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAc.Domain.Alerts;

public sealed class Alert : EntityBase
{
#pragma warning disable CS8618
    private Alert() { }
#pragma warning restore CS8618

    private Alert(AlertType alertType, string deviceSerialNumber, DateTimeOffset reportedDateTime, string message)
    {
        DeviceSerialNumber = deviceSerialNumber;
        ReportedDateTime = reportedDateTime;
        Message = message;
        AlertType = alertType;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AlertId { get; private set; }

    public AlertType AlertType { get; private set; }

    public string Message { get; private set; }

    public AlertState AlertState { get; private set; } = AlertState.New;

    public DateTimeOffset CreatedDateTime { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ReportedDateTime { get; private set; }

    public DateTimeOffset? LastReportedDateTime { get; private set; }

    public string DeviceSerialNumber { get; private set; }

    public static Alert CreateNew(
        AlertType alertType, 
        string alertMessage, 
        string deviceSerialNumber,
        DateTimeOffset reportDate)
    {
        return new Alert(alertType, deviceSerialNumber, reportDate, alertMessage);
    }

    public void Update(AlertState alertState, string message, DateTimeOffset lastReportedDateTime)
    {
        LastReportedDateTime = lastReportedDateTime;
        Message = message;
        AlertState = alertState;
    }
}