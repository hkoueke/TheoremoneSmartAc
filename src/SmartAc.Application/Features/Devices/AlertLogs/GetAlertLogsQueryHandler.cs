using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAc.Application.Abstractions.Messaging;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Contracts;
using SmartAc.Application.Dto;
using SmartAc.Application.Specifications.Alerts;
using SmartAc.Application.Specifications.Devices;
using SmartAc.Domain;
using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;
using System.Linq.Expressions;

// ReSharper disable PossibleMultipleEnumeration

namespace SmartAc.Application.Features.Devices.AlertLogs;

internal sealed class GetAlertLogsQueryHandler : IQueryHandler<GetAlertLogsQuery, PagedList<LogItem>>
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

        var hasMatchingAlerts =
            await _alertRepository.ContainsAsync(new AlertsMatchingStateSpecification(predicate), cancellationToken);

        if (!hasMatchingAlerts)
        {
            return PagedList<LogItem>.ToPagedList(
                Enumerable.Empty<LogItem>(),
                request.Params.Page,
                request.Params.PageSize);
        }

        var skip = request.Params.PageSize * (request.Params.Page - 1);
        var take = request.Params.PageSize;

        var specification = alertState switch
        {
            not null => new DevicesWithAlertsSpecification(request.SerialNumber, alertState.Value, skip, take),
            _ => new DevicesWithAlertsSpecification(request.SerialNumber, skip, take)
        };

        var query = _deviceRepository.GetQueryable(specification);
        var device = await _mapper.ProjectTo<DeviceLogDto>(query).SingleAsync(cancellationToken);
        var logItems = ComputeLogItems(device, cancellationToken);

        return PagedList<LogItem>.ToPagedList(logItems, request.Params.Page, request.Params.PageSize);
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
                    MinValue = GetMinOrMax(readings, alert.AlertType, ValueType.Minimum),
                    MaxValue = GetMinOrMax(readings, alert.AlertType, ValueType.Maximum),
                });
    }

    private static decimal GetMinOrMax(IEnumerable<DeviceReading> readings, AlertType alertType, ValueType valueType)
    {
        return alertType switch
        {
            AlertType.OutOfRangeTemp => valueType == ValueType.Minimum
                ? readings.Min(x => x.Temperature)
                : readings.Max(x => x.Temperature),

            AlertType.OutOfRangeCo => valueType == ValueType.Minimum
                ? readings.Min(x => x.CarbonMonoxide)
                : readings.Max(x => x.CarbonMonoxide),

            AlertType.OutOfRangeHumidity => valueType == ValueType.Minimum
                ? readings.Min(x => x.Humidity)
                : readings.Max(x => x.Humidity),

            AlertType.DangerousCoLevel => valueType == ValueType.Minimum
                ? readings.Min(x => x.CarbonMonoxide)
                : readings.Max(x => x.CarbonMonoxide),

            _ => 0m
        };
    }

    private enum ValueType
    {
        Minimum,
        Maximum
    }
}
