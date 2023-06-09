using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Options;
using SmartAc.Application.Specifications.Alerts;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Infrastructure.BackgroundJobs.Handlers;

internal sealed class AlertResolverHandler : AbstractHandler<DeviceReading>
{
    private readonly IRepository<Alert> _repository;
    private readonly SensorParams _sensorParams;

    public AlertResolverHandler(IRepository<Alert> repository, IOptionsMonitor<SensorParams> options)
    {
        _repository = repository;
        _sensorParams = options.CurrentValue;
    }

    public override async Task Handle(DeviceReading item, CancellationToken cancellationToken)
    {
        var specification = new AlertsMatchingStateSpecification(item.DeviceSerialNumber, AlertState.New);

        List<Alert> alerts = await
            _repository.GetQueryable(specification)
                       .ToListAsync(cancellationToken);

        foreach (var alert in alerts.Where(alert => AlertHelpers.IsResolved(item, alert.AlertType, _sensorParams)))
        {
            alert.Update(AlertState.Resolved, alert.Message, item.RecordedDateTime);
            _repository.Update(alert);
        }

        Next?.Handle(item, cancellationToken).ConfigureAwait(false);
    }
}