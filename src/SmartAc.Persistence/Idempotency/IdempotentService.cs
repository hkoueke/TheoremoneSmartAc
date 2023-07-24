using Microsoft.EntityFrameworkCore;
using SmartAc.Application.Abstractions.Services;
using SmartAc.Domain;
using SmartAc.Persistence.Repositories;

namespace SmartAc.Persistence.Idempotency;

public sealed class IdempotentService : IIdempotentService
{
    private readonly SmartAcContext _context;

    public IdempotentService(SmartAcContext context)
    {
        _context = context;
    }

    public Task<bool> RequestExistsAsync(string hashString, CancellationToken cancellationToken = default)
        => _context.Set<IdempotentRequestEntry>().AnyAsync(ir => ir.HashString == hashString, cancellationToken);

    public Task CreateRequestEntryAsync(string hashString, string requestName, CancellationToken cancellationToken = default)
    {
        var entry = new IdempotentRequestEntry
        {
            HashString = hashString,
            FromCommand = requestName
        };

        _context.Add(entry);

        _context.SaveChangesAsync(cancellationToken);

        return Task.CompletedTask;
    }
}