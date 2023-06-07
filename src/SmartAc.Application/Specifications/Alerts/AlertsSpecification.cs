using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain.Alerts;

namespace SmartAc.Application.Specifications.Alerts;

public sealed class AlertsSpecification : BaseSpecification<Alert>
{
    public AlertsSpecification(string deviceSerialNumber, AlertType alertType)
        : base(x => x.DeviceSerialNumber == deviceSerialNumber && x.AlertType == alertType)
    {
        ApplyOrderBy(x => x.ReportedDateTime);
    }
}