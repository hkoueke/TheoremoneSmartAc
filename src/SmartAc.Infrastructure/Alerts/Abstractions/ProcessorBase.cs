using Microsoft.Extensions.Options;
using SmartAc.Application.Options;
using SmartAc.Domain.Devices;

namespace SmartAc.Infrastructure.Alerts.Abstractions;

internal abstract class ProcessorBase
{
    private ProcessorBase? _nextProcessor;
    protected readonly SensorOptions SensorOptions;

    protected ProcessorBase(IOptionsMonitor<SensorOptions> options) => SensorOptions = options.CurrentValue;

    public ProcessorBase SetNext(ProcessorBase processor)
    {
        _nextProcessor = processor;
        return processor;
    }

    public virtual void Process(Device device) => _nextProcessor?.Process(device);
}