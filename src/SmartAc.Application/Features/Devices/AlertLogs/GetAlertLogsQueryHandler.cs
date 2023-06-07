using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Contracts;
using SmartAc.Application.Dto;
using SmartAc.Application.Specifications.Alerts;
using SmartAc.Application.Specifications.Devices;
using SmartAc.Domain;
using SmartAc.Domain.Alerts;

// ReSharper disable PossibleMultipleEnumeration

namespace SmartAc.Application.Features.Devices.AlertLogs;

internal sealed class GetAlertLogsQueryHandler : IRequestHandler<GetAlertLogsQuery, PagedList<LogItem>>
{
    private readonly IRepository<Alert> _alertRepository;
    private readonly IRepository<Device> _deviceRepository;
    private readonly IMapper _mapper;

    public GetAlertLogsQueryHandler(
        IRepository<Device> deviceRepository,
        IRepository<Alert> alertRepository,
        IMapper mapper)
    {
        _deviceRepository = deviceRepository;
        _alertRepository = alertRepository;
        _mapper = mapper;
    }

    public async Task<PagedList<LogItem>> Handle(GetAlertLogsQuery request, CancellationToken cancellationToken)
    {
        AlertState? alertState = request.Params.Filter switch
        {
            FilterType.New => AlertState.New,
            FilterType.Resolved => AlertState.Resolved,
            _ => null
        };

        Expression<Func<Alert, bool>> predicate = alert => alertState == null
                ? alert.DeviceSerialNumber == request.SerialNumber
                : alert.DeviceSerialNumber == request.SerialNumber && alert.AlertState == alertState;

        var itemsCount = await
            _alertRepository.CountAsync(new AlertsMatchingStateSpecification(predicate), cancellationToken);

        if (itemsCount == 0)
        {
            return new PagedList<LogItem>(
                Enumerable.Empty<LogItem>(),
                0,
                request.Params.PageNumber,
                request.Params.PageSize);
        }

        var skip = request.Params.PageSize * (request.Params.PageNumber - 1);
        var take = request.Params.PageSize;

        var specification = alertState is null
            ? new DevicesWithAlertsSpecification(request.SerialNumber, skip, take)
            : new DevicesWithAlertsSpecification(request.SerialNumber, alertState.Value, skip, take);

        IQueryable<Device> query = _deviceRepository.GetQueryable(specification);
        DeviceLogDto? device = await _mapper.ProjectTo<DeviceLogDto>(query).SingleAsync(cancellationToken);
        IEnumerable<LogItem> logItems = ComputeLogItems(device, cancellationToken);

        return new PagedList<LogItem>(logItems, itemsCount, request.Params.PageNumber, request.Params.PageSize);
    }

    private static IEnumerable<LogItem> ComputeLogItems(in DeviceLogDto device, CancellationToken cancellationToken)
    {
        return device.Alerts
            .AsParallel().AsOrdered()
            .WithDegreeOfParallelism(2)
            .WithCancellation(cancellationToken)
            .GroupJoin(
                device.DeviceReadings.AsParallel(),
                alert => alert.DeviceSerialNumber,
                reading => reading.DeviceSerialNumber,
                (alert, readings) => new LogItem
                {
                    AlertType = alert.AlertType,
                    Message = alert.Message,
                    AlertState = alert.AlertState,
                    DateTimeCreated = alert.CreatedDateTime,
                    DateTimeReported = alert.ReportedDateTime,
                    DateTimeLastReported = alert.LastReportedDateTime,
                    MinValue = alert.AlertType switch
                    {
                        AlertType.OutOfRangeTemp => readings.Min(x => x.Temperature),
                        AlertType.OutOfRangeCo => readings.Min(x => x.CarbonMonoxide),
                        AlertType.OutOfRangeHumidity => readings.Min(x => x.Humidity),
                        AlertType.DangerousCoLevel => readings.Min(x => x.CarbonMonoxide),
                        _ => 0m
                    },
                    MaxValue = alert.AlertType switch
                    {
                        AlertType.OutOfRangeTemp => readings.Max(x => x.Temperature),
                        AlertType.OutOfRangeCo => readings.Max(x => x.CarbonMonoxide),
                        AlertType.OutOfRangeHumidity => readings.Max(x => x.Humidity),
                        AlertType.DangerousCoLevel => readings.Max(x => x.CarbonMonoxide),
                        _ => 0m
                    }
                });
    }
}    
