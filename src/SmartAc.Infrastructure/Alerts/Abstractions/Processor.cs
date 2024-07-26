﻿using SmartAc.Application.Options;
using SmartAc.Domain.Devices;

namespace SmartAc.Infrastructure.Alerts.Abstractions;

internal abstract class Processor : HandlerBase<Device>
{
    protected Processor(SensorOptions options) : base(options)
    {
    }

    public override void Handle(Device item)
    {
        Process(item);
        base.Handle(item);
    }

    public abstract void Process(Device device);
}

