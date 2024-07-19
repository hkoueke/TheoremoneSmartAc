using SmartAc.Application.Abstractions.Messaging;
using SmartAc.Application.Contracts;
using SmartAc.Domain.Services.Reporting;

namespace SmartAc.Application.Features.Devices.Reporting;

public sealed record GetAlertReportsQuery(string SerialNumber, QueryParams Params) : IQuery<PagedList<AlertReport>>;