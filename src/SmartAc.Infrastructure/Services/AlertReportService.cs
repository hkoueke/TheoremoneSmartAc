using Microsoft.EntityFrameworkCore;
using SmartAc.Application.Abstractions.Reporting;
using SmartAc.Domain.Alerts;
using SmartAc.Persistence;

namespace SmartAc.Infrastructure.Services;

internal sealed class AlertReportService : IAlertReportService
{
    private readonly SmartAcContext _context;

    public AlertReportService(SmartAcContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AlertReport>> ComputeAlertReportsAsync(
        string deviceSerialNumber,
        AlertState? alertState,
        CancellationToken cancellationToken = default)
    {
        var queryData =
            from alert in _context.Alerts.AsNoTracking()
            where !alertState.HasValue
                ? alert.DeviceSerialNumber == deviceSerialNumber
                : alert.DeviceSerialNumber == deviceSerialNumber && alert.AlertState == alertState
            join reading in _context.DeviceReadings.AsNoTracking() on alert.DeviceSerialNumber equals reading.DeviceSerialNumber
            group new { alert, reading } by alert.AlertId into grouped
            select new
            {
                AlertId = grouped.Key,
                Alert = grouped.Select(x => new
                {
                    x.alert.AlertId,
                    x.alert.DeviceSerialNumber,
                    x.alert.CreatedDateTimeUtc,
                    x.alert.ReportedDateTimeUtc,
                    x.alert.LastReportedDateTimeUtc,
                    x.alert.AlertState,
                    x.alert.AlertType,
                    x.alert.Message
                }).First(),
                Readings = grouped.Select(x => new
                {
                    x.reading.DeviceReadingId,
                    x.reading.Health,
                    x.reading.Temperature,
                    x.reading.CarbonMonoxide,
                    x.reading.Humidity
                })
            };

        var queryResult = await queryData.ToListAsync(cancellationToken);

        var reports = queryResult
            .Select(x => new AlertReport
            {
                DeviceSerialNumber = x.Alert.DeviceSerialNumber,
                AlertType = x.Alert.AlertType,
                AlertState = x.Alert.AlertState,
                CreatedDateTimeUtc = x.Alert.CreatedDateTimeUtc,
                ReportedDateTimeUtc = x.Alert.ReportedDateTimeUtc,
                LastReportedDateTimeUtc = x.Alert.LastReportedDateTimeUtc,
                Message = x.Alert.Message,
                MinValue = x.Alert.AlertType switch
                {
                    AlertType.OutOfRangeTemp => x.Readings.AsParallel().Min(x => x.Temperature),
                    AlertType.OutOfRangeCo => x.Readings.AsParallel().Min(x => x.CarbonMonoxide),
                    AlertType.OutOfRangeHumidity => x.Readings.AsParallel().Min(x => x.Humidity),
                    AlertType.DangerousCoLevel => x.Readings.AsParallel().Min(x => x.CarbonMonoxide),
                    _ => 0m,
                },
                MaxValue = x.Alert.AlertType switch
                {
                    AlertType.OutOfRangeTemp => x.Readings.AsParallel().Max(x => x.Temperature),
                    AlertType.OutOfRangeCo => x.Readings.AsParallel().Max(x => x.CarbonMonoxide),
                    AlertType.OutOfRangeHumidity => x.Readings.AsParallel().Max(x => x.Humidity),
                    AlertType.DangerousCoLevel => x.Readings.AsParallel().Max(x => x.CarbonMonoxide),
                    _ => 0m,
                },
            })
            .OrderByDescending(x => x.ReportedDateTimeUtc)
            .ToList();

        return reports;
    }
}
