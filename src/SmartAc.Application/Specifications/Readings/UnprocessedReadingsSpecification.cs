using System.Linq.Expressions;
using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Application.Specifications.Readings;

public sealed class UnprocessedReadingsSpecification : BaseSpecification<DeviceReading>
{
    private UnprocessedReadingsSpecification(Expression<Func<DeviceReading, bool>> predicate) : base(predicate)
    {
    }

    public UnprocessedReadingsSpecification() : this(r => r.ProcessedDateTime == null)
    {
        ApplyOrderBy(r => r.RecordedDateTime);
    }

    public UnprocessedReadingsSpecification(int take) : this() => ApplyPaging(0, take);
}