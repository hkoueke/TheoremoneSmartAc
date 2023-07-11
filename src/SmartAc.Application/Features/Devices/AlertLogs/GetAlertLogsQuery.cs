using SmartAc.Application.Abstractions.Messaging;
using SmartAc.Application.Contracts;

namespace SmartAc.Application.Features.Devices.AlertLogs;

public sealed record GetAlertLogsQuery(string SerialNumber, QueryParams Params) : IQuery<PagedList<LogItem>>;