using Microsoft.EntityFrameworkCore;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Services.Reporting;

namespace SmartAc.Persistence.Services;

internal sealed class AlertReportService : IAlertReportService
{
    private readonly SmartAcContext _context;

    public AlertReportService(SmartAcContext context)
    {
        _context = context;
    }

    public async Task<List<AlertReport>> ComputeAlertReportsAsync(
        string deviceSerialNumber,
        AlertState? alertState,
        CancellationToken cancellationToken = default)
    {
        var queryData =
            from alert in _context.Alerts
            join reading in _context.DeviceReadings on alert.DeviceSerialNumber equals reading.DeviceSerialNumber
            where !alertState.HasValue
                ? alert.DeviceSerialNumber == deviceSerialNumber
                : alert.DeviceSerialNumber == deviceSerialNumber && alert.AlertState == alertState
            orderby alert.ReportedDateTimeUtc descending
            select new
            {
                alert.AlertId,
                alert.DeviceSerialNumber,
                alert.CreatedDateTimeUtc,
                alert.ReportedDateTimeUtc,
                alert.LastReportedDateTimeUtc,
                alert.AlertState,
                alert.AlertType,
                alert.Message,
                reading.DeviceReadingId,
                reading.Health,
                reading.Temperature,
                reading.CarbonMonoxide,
                reading.Humidity
            };

        var queryResult = await queryData.ToListAsync(cancellationToken);

        List<AlertReport> reports = queryResult
        .GroupBy(x => x.AlertId)
        .Select(g => new
        {
            g.First().DeviceSerialNumber,
            g.First().AlertType,
            g.First().AlertState,
            g.First().CreatedDateTimeUtc,
            g.First().ReportedDateTimeUtc,
            g.First().LastReportedDateTimeUtc,
            g.First().Message,
            MinValue = g.First().AlertType switch
            {
                AlertType.OutOfRangeTemp => g.Min(x => x.Temperature),
                AlertType.OutOfRangeCo => g.Min(x => x.CarbonMonoxide),
                AlertType.OutOfRangeHumidity => g.Min(x => x.Humidity),
                AlertType.DangerousCoLevel => g.Min(x => x.CarbonMonoxide),
                _ => 0m,
            },
            MaxValue = g.First().AlertType switch
            {
                AlertType.OutOfRangeTemp => g.Max(x => x.Temperature),
                AlertType.OutOfRangeCo => g.Max(x => x.CarbonMonoxide),
                AlertType.OutOfRangeHumidity => g.Max(x => x.Humidity),
                AlertType.DangerousCoLevel => g.Max(x => x.CarbonMonoxide),
                _ => 0m,
            }
        })
        .Select(g => new AlertReport
        {
            DeviceSerialNumber = g.DeviceSerialNumber,
            AlertType = g.AlertType,
            AlertState = g.AlertState,
            CreatedDateTimeUtc = g.CreatedDateTimeUtc,
            ReportedDateTimeUtc = g.ReportedDateTimeUtc,
            LastReportedDateTimeUtc = g.LastReportedDateTimeUtc,
            Message = g.Message,
            MinValue = g.MinValue,
            MaxValue = g.MaxValue
        })
        .ToList();

        return reports;
    }
}
