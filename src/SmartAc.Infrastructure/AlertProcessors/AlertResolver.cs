using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Options;
using SmartAc.Application.Specifications.Alerts;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;
using SmartAc.Infrastructure.AlertResolvers;

namespace SmartAc.Infrastructure.AlertProcessors;

internal sealed class AlertResolver : Processor
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Alert> _repository;
    private readonly SensorParams _sensorParams;

    public AlertResolver(
        IUnitOfWork unitOfWork,
        IRepository<Alert> repository,
        SensorParams sensorParams)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
        _sensorParams = sensorParams;
    }

    public override async Task ProcessAsync(DeviceReading reading, CancellationToken cancellationToken = default)
    {
        await TryResolveAlertsAsync(reading, cancellationToken);
        await base.ProcessAsync(reading, cancellationToken);
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

        foreach (var alert in _repository
                     .GetQueryable(specification)
                     .Where(a => resolver.IsResolved(reading, a.AlertType, _sensorParams)))
        {
            alert.Update(AlertState.Resolved, alert.Message, reading.RecordedDateTime);
            _repository.Update(alert);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}