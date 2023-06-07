using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Specifications.Readings;
using SmartAc.Domain.DeviceReadings;
using SmartAc.Infrastructure.BackgroundJobs.Handlers;
using SmartAc.Infrastructure.Options;

namespace SmartAc.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
internal sealed class ProcessNewReadingsJob : IJob
{
    private readonly IRepository<DeviceReading> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AlertProducerHandler _alertProducerHandler;
    private readonly int _batchSize;

    public ProcessNewReadingsJob(
        IUnitOfWork unitOfWork,
        IRepository<DeviceReading> repository,
        IOptionsMonitor<BackgroundJobParams> options,
        AlertProducerHandler alertProducerHandler,
        AlertResolverHandler alertResolverHandler)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _alertProducerHandler = alertProducerHandler;
        _alertProducerHandler.SetNextHandler(alertResolverHandler);
        _batchSize = options.CurrentValue.BatchSize;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (!await HasUnprocessedReadings(context.CancellationToken))
        {
            return;
        }

        var specification = new UnprocessedReadingsSpecification(_batchSize);

        IEnumerable<DeviceReading> readings = await
            _repository.GetQueryable(specification)
                       .ToListAsync(context.CancellationToken);

        foreach (var reading in readings)
        {
            await _alertProducerHandler.Handle(reading, context.CancellationToken);
            reading.MarkAsProcessed(DateTimeOffset.UtcNow);
            _repository.Update(reading);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }

    private async Task<bool> HasUnprocessedReadings(CancellationToken cancellationToken) =>
        await _repository.ContainsAsync(new UnprocessedReadingsSpecification(), cancellationToken);
}