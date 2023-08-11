using Microsoft.Extensions.Options;
using Quartz;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Options;
using SmartAc.Application.Specifications.Readings;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;
using SmartAc.Infrastructure.AlertProcessors;
using SmartAc.Infrastructure.Options;

namespace SmartAc.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
internal sealed class ReadingProcessorBackgroundJob : IJob
{
    private readonly IRepository<DeviceReading> _deviceRepository;
    private readonly IRepository<Alert> _alertRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SensorParams _sensorParams;
    private readonly int _batchSize;

    public ReadingProcessorBackgroundJob(
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
            return;

        var specification = new UnprocessedReadingsSpecification(_batchSize);

        var processor = new Processor();

        processor
            .SetNext(new AlertProcessor(_unitOfWork, _alertRepository, _sensorParams))
            .SetNext(new AlertResolver(_unitOfWork, _alertRepository, _sensorParams));

        foreach (var reading in _deviceRepository.GetQueryable(specification))
        {
            await processor.ProcessAsync(reading, context.CancellationToken);
            reading.MarkAsProcessed(DateTimeOffset.UtcNow);
            _deviceRepository.Update(reading);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }

    private async Task<bool> HasNewReadings(CancellationToken cancellationToken) =>
        await _deviceRepository.ContainsAsync(new UnprocessedReadingsSpecification(), cancellationToken);
}