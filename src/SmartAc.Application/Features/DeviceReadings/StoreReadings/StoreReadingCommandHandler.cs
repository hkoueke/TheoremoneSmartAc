using MediatR;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Helpers;
using SmartAc.Domain;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Application.Features.DeviceReadings.StoreReadings;

internal sealed class StoreReadingCommandHandler : IRequestHandler<StoreReadingCommand>
{
    private readonly IRepository<DeviceReading> _readingRepository;
    private readonly IRepository<HashStore> _hashRepository;

    public StoreReadingCommandHandler(IRepository<DeviceReading> repository, IRepository<HashStore> hashRepository)
    {
        _readingRepository = repository;
        _hashRepository = hashRepository;
    }

    public async Task Handle(StoreReadingCommand request, CancellationToken cancellationToken)
    {
        if (await _hashRepository.ContainsAsync(hs => hs.HashCode == request.GetHexString(), cancellationToken))
        {
            return;
        }

        IEnumerable<DeviceReading> readings =
            request.Readings
                .Select(r => r.ToDeviceReading(request.DeviceSerialNumber))
                .ToList();

        _readingRepository.AddRange(readings);
    }
}