using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using SmartAc.Application.Options;
using SmartAc.Domain.Abstractions;
using SmartAc.Domain.Devices;
using SmartAc.Infrastructure.Alerts;
using SmartAc.Infrastructure.Options;

namespace SmartAc.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
internal sealed class DeviceReadingProcessorJob : IJob
{
    private readonly IDeviceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptionsMonitor<SensorOptions> _sensorOptions;
    private readonly int _batchSize;

    public DeviceReadingProcessorJob(
        IDeviceRepository repository,
        IUnitOfWork unitOfWork,
        IOptionsMonitor<SensorOptions> sensorOptions,
        IOptionsMonitor<JobOptions> jobOptions)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _sensorOptions = sensorOptions;
        _batchSize = jobOptions.CurrentValue.BatchSize;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (!TryGetDevicesWithUnprocessedReadings(out var devices))
            return;

        var processor = Helpers.GetProcessor(_sensorOptions);

        foreach (var device in devices)
        {
            processor.Process(device);
            _repository.Update(device);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }

    private bool TryGetDevicesWithUnprocessedReadings(out IEnumerable<Device> devices)
    {
        var devicesQuery = _repository
            .GetQueryable()
            .Include(d => d.Alerts)
            .Include(d => d.DeviceReadings.Where(dr => !dr.ProcessedOnDateTimeUtc.HasValue))
            .Where(d => d.DeviceReadings.Any(dr => !dr.ProcessedOnDateTimeUtc.HasValue)) // Filter devices based on unprocessed readings
            .OrderBy(x => x.SerialNumber)
            .Take(_batchSize);

        devices = devicesQuery.Any() ? devicesQuery : Enumerable.Empty<Device>();

        return devices.Any();
    }
}