using SmartAc.Application.Contracts;

namespace SmartAc.Application.Features.Devices.Reporting;

public sealed record QueryParams
{
    private const int MaxPageSize = 50;

    private readonly int _pageSize = 50;

    private readonly int _pageNumber = 1;

    public int Page
    {
        get => _pageNumber;
        init => _pageNumber = value <= 0 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public FilterType? Filter { get; init; }

}