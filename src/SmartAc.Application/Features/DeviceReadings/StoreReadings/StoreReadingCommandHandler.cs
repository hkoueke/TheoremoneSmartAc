using MediatR;
using SmartAc.Application.Abstractions.Messaging;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Application.Features.DeviceReadings.StoreReadings;

internal sealed class StoreReadingCommandHandler : ICommandHandler<StoreReadingCommand, Unit>
{
    private readonly IRepository<DeviceReading> _repository;

    public StoreReadingCommandHandler(IRepository<DeviceReading> repository)
    {
        _repository = repository;
    }

    public Task<Unit> Handle(StoreReadingCommand request, CancellationToken cancellationToken)
    {
        IEnumerable<DeviceReading> readings =
            request.Readings
                .Select(r => r.ToDeviceReading(request.DeviceSerialNumber))
                .ToList();

        _repository.AddRange(readings);

        return Task.FromResult(Unit.Value);
    }
}