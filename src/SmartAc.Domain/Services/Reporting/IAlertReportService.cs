using SmartAc.Domain.Alerts;

namespace SmartAc.Domain.Services.Reporting;

public interface IAlertReportService
{
    public Task<List<AlertReport>> ComputeAlertReportsAsync(
        string deviceSerialNumber,
        AlertState? alertState,
        CancellationToken cancellationToken = default);
}
