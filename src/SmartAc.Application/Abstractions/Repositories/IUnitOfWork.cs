using Microsoft.EntityFrameworkCore.Storage;

namespace SmartAc.Application.Abstractions.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    IDbContextTransaction Transaction { get; }
}