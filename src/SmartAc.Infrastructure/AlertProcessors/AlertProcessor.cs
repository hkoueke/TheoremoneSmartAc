using Microsoft.EntityFrameworkCore;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Options;
using SmartAc.Application.Specifications.Alerts;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;
using SmartAc.Infrastructure.Extensions;

namespace SmartAc.Infrastructure.AlertProcessors;

internal sealed class AlertProcessor : Processor
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Alert> _alertRepository;
    private readonly SensorParams _sensorParams;

    public AlertProcessor(
        IUnitOfWork unitOfWork,
        IRepository<Alert> alertRepository,
        SensorParams sensorParams)
    {
        _unitOfWork = unitOfWork;
        _alertRepository = alertRepository;
        _sensorParams = sensorParams;
    }

    public override async Task ProcessAsync(DeviceReading reading, CancellationToken cancellationToken = default)
    {
        await TryProduceAlertsAsync(reading, cancellationToken);
        await base.ProcessAsync(reading, cancellationToken);
    }

    private async Task TryProduceAlertsAsync(DeviceReading reading, CancellationToken cancellationToken)
    {
        var alerts =
            reading.GetAlerts(_sensorParams)
                   .OrderBy(x => x.ReportedDateTime);

        foreach (var alert in alerts)
        {
            var specification = new AlertsSpecification(alert.DeviceSerialNumber, alert.AlertType);

            if (!await _alertRepository.ContainsAsync(specification, cancellationToken))
            {
                _alertRepository.Add(alert);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                continue;
            }

            var alertFromDb = await
                _alertRepository
                    .GetQueryable(specification)
                    .FirstAsync(cancellationToken);

            var diff = Math.Abs((alert.ReportedDateTime - alertFromDb.ReportedDateTime).TotalMinutes);

            var alertState = (diff < _sensorParams.ReadingAgeInMinutes) switch
            {
                true when alertFromDb.AlertState == AlertState.Resolved => AlertState.New,
                true when alertFromDb.AlertState == AlertState.New => alertFromDb.AlertState,
                _ => AlertState.Resolved
            };

            alertFromDb.Update(alertState, alert.Message, alert.ReportedDateTime);

            _alertRepository.Update(alertFromDb);

            if (alertState == AlertState.Resolved)
            {
                _alertRepository.Add(alert);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}