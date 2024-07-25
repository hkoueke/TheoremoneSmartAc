using Microsoft.Extensions.Options;
using SmartAc.Application.Options;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.Devices;
using SmartAc.Infrastructure.Alerts.Abstractions;
using SmartAc.Infrastructure.Alerts.Resolvers.Sets;

namespace SmartAc.Infrastructure.Alerts.Resolvers;

internal sealed class AlertResolver : ProcessorBase
{
    public AlertResolver(IOptionsMonitor<SensorOptions> options) : base(options)
    {
    }

    public override void Process(Device device)
    {
        TryResolveAlerts(device);
        base.Process(device);
    }

    private void TryResolveAlerts(Device device)
    {
        if (!device.Alerts.Any(x => x.AlertState == AlertState.New))
        {
            return;
        }

        var resolver = Helpers.GetResolver();

        foreach (var reading in device.DeviceReadings)
        {
            var alert = device.Alerts
                .Where(a => a.AlertState == AlertState.New)
                .Where(x => resolver.IsResolved(reading, x.AlertType, SensorOptions))
                .FirstOrDefault();

            alert?.UpdateState(AlertState.Resolved, reading.RecordedDateTimeUtc);
        }
    }
}