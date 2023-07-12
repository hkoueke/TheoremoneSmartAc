using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Options;
using SmartAc.Application.Specifications.Alerts;
using SmartAc.Application.Specifications.Readings;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;
using SmartAc.Infrastructure.Options;
using SmartAc.Infrastructure.Resolvers;

namespace SmartAc.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
internal sealed class ProcessNewReadingsJob : IJob
{
    private readonly IRepository<DeviceReading> _deviceRepository;
    private readonly IRepository<Alert> _alertRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SensorParams _sensorParams;
    private readonly int _batchSize;

    public ProcessNewReadingsJob(
        IUnitOfWork unitOfWork,
        IRepository<DeviceReading> deviceRepository,
        IRepository<Alert> alertRepository,
        IOptionsMonitor<BackgroundJobParams> options,
        IOptionsMonitor<SensorParams> sensorParams)
    {
        _unitOfWork = unitOfWork;
        _deviceRepository = deviceRepository;
        _alertRepository = alertRepository;
        _batchSize = options.CurrentValue.BatchSize;
        _sensorParams = sensorParams.CurrentValue;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (!await HasNewReadings(context.CancellationToken))
        {
            return;
        }

        var specification = new UnprocessedReadingsSpecification(_batchSize);

        IEnumerable<DeviceReading> readings = await
            _deviceRepository.GetQueryable(specification)
                             .ToListAsync(context.CancellationToken);

        foreach (var reading in readings)
        {
            await TryProduceAlertsAsync(reading, context.CancellationToken);
            await TryResolveAlertsAsync(reading, context.CancellationToken);

            reading.MarkAsProcessed(DateTimeOffset.UtcNow);

            _deviceRepository.Update(reading);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }

    private async Task<bool> HasNewReadings(CancellationToken cancellationToken) =>
        await _deviceRepository.ContainsAsync(new UnprocessedReadingsSpecification(), cancellationToken);

    private async Task TryProduceAlertsAsync(DeviceReading reading, CancellationToken cancellationToken)
    {
        IEnumerable<Alert> alerts =
            reading.GetAlerts(_sensorParams)
                   .OrderBy(x => x.ReportedDateTime)
                   .ToList();

        if (!alerts.Any())
        {
            return;
        }

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
                _alertRepository.GetQueryable(specification)
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

    private async Task TryResolveAlertsAsync(DeviceReading reading, CancellationToken cancellationToken)
    {
        var resolver = new Resolver();
        resolver
            .SetNext(new TempInRange())
            .SetNext(new SensorHealthy())
            .SetNext(new MonoxideLevelSafe())
            .SetNext(new MonoxideInRange())
            .SetNext(new HumidityInRange());

        var specification = new AlertsMatchingStateSpecification(reading.DeviceSerialNumber, AlertState.New);

        var alerts = await
            _alertRepository.GetQueryable(specification)
                            .ToListAsync(cancellationToken);

        foreach (var alert in alerts.Where(a => resolver.IsResolved(reading, a.AlertType, _sensorParams)))
        {
            alert.Update(AlertState.Resolved, alert.Message, reading.RecordedDateTime);
            _alertRepository.Update(alert);
        }
    }
}