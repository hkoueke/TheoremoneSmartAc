namespace SmartAc.Application.Abstractions.Services;

public interface IIdempotentService
{
    Task<bool> RequestExistsAsync(string hashString, CancellationToken cancellationToken = default);

    Task CreateRequestEntryAsync(string hashString, string requestName, CancellationToken cancellationToken = default);
}