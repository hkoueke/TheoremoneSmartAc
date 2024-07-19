using Microsoft.Extensions.Options;
using SmartAc.Application.Options;
using SmartAc.Domain.Devices;

namespace SmartAc.Infrastructure.AlertProcessors;

internal abstract class Processor
{
    private Processor? _nextProcessor;
    protected SensorOptions SensorOptions;

    protected Processor(IOptionsMonitor<SensorOptions> options) => SensorOptions = options.CurrentValue;

    public Processor SetNext(Processor processor)
    {
        _nextProcessor = processor;
        return processor;
    }

    public virtual void Process(Device device) => _nextProcessor?.Process(device);
}