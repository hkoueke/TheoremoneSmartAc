using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Infrastructure.AlertProcessors;

internal class Processor
{
    private Processor? _nextProcessor;

    public Processor SetNext(Processor processor)
    {
        _nextProcessor = processor;
        return processor;
    }

    public virtual Task ProcessAsync(DeviceReading reading, CancellationToken cancellationToken = default)
        => _nextProcessor?.ProcessAsync(reading, cancellationToken) ?? Task.CompletedTask;
}