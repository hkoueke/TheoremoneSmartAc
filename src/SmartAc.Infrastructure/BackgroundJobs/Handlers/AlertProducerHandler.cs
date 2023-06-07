using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Options;
using SmartAc.Application.Specifications.Alerts;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Infrastructure.BackgroundJobs.Handlers;

internal sealed class AlertProducerHandler : AbstractHandler<DeviceReading>
{
    private readonly SensorParams _sensorParams;
    private readonly IRepository<Alert> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AlertProducerHandler(IOptions<SensorParams> options, IRepository<Alert> repository, IUnitOfWork unitOfWork)
    {
        _sensorParams = options.Value;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public override async Task Handle(DeviceReading item, CancellationToken cancellationToken)
    {
        IEnumerable<Alert> alerts = 
            item.GetAlerts(_sensorParams)
                .OrderBy(x => x.ReportedDateTime)
                .ToList();

        if (!alerts.Any())
        {
            Next?.Handle(item, cancellationToken);
            return;
        }

        foreach (var alert in alerts)
        {
            var specification = new AlertsSpecification(alert.DeviceSerialNumber, alert.AlertType);

            if (!await _repository.ContainsAsync(specification, cancellationToken))
            {
                _repository.Add(alert);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                continue;
            }

            Alert alertFromDb = await
                _repository.GetQueryable(specification)
                           .FirstAsync(cancellationToken);

            var diff = Math.Abs((alert.ReportedDateTime - alertFromDb.ReportedDateTime).TotalMinutes);

            var alertState = (diff <= _sensorParams.ReadingAgeInMinutes) switch
            {
                true when alertFromDb.AlertState == AlertState.Resolved => AlertState.New,
                _ => AlertState.Resolved
            };

            alertFromDb.Update(alertState, alert.Message, alert.ReportedDateTime);

            _repository.Update(alertFromDb);

            if (alertState == AlertState.Resolved)
            {
                _repository.Add(alert);
            }

            //await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        Next?.Handle(item, cancellationToken).ConfigureAwait(false);
    }
}