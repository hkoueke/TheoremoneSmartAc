﻿using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;
using SmartAc.Domain.Alerts;

namespace SmartAc.Application.Specifications.Devices;

public sealed class DevicesWithAlertsSpecification : BaseSpecification<Device>
{
    public DevicesWithAlertsSpecification()
    {
        AddInclude(x => x.Alerts);
        AddInclude(x => x.DeviceReadings);
    }

    public DevicesWithAlertsSpecification(string serialNumber, int skip, int take)
        : base(x => x.SerialNumber == serialNumber)
    {
        AddInclude(x => 
            x.Alerts
                .OrderBy(a => a.ReportedDateTime)
                .Skip(skip)
                .Take(take));

        AddInclude(x => x.DeviceReadings);
    }

    public DevicesWithAlertsSpecification(string serialNumber, AlertState alertState, int skip, int take)
        : base(x => x.SerialNumber == serialNumber)
    {
        AddInclude(x => 
            x.Alerts
                .Where(a => a.AlertState == alertState)
                .OrderBy(a => a.ReportedDateTime)
                .Skip(skip)
                .Take(take));

        AddInclude(x => x.DeviceReadings);
    }
}