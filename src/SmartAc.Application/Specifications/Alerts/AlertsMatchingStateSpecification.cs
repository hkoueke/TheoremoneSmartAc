using System.Linq.Expressions;
using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain.Alerts;

namespace SmartAc.Application.Specifications.Alerts;

public sealed class AlertsMatchingStateSpecification : BaseSpecification<Alert>
{
    public AlertsMatchingStateSpecification(string deviceSerialNumber, AlertState alertState)
        : base(x =>
            x.DeviceSerialNumber == deviceSerialNumber &&
            x.AlertState == alertState)
    {
        ApplyOrderBy(x => x.ReportedDateTime);
    }

    public AlertsMatchingStateSpecification(Expression<Func<Alert, bool>> predicate) : base(predicate)
    {
        ApplyOrderBy(x => x.ReportedDateTime);
    }
}