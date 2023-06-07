using Microsoft.EntityFrameworkCore.Storage;
using SmartAc.Application.Abstractions.Repositories;

namespace SmartAc.Persistence.Repositories;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly SmartAcContext _context;

    public UnitOfWork(SmartAcContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => 
        _context.SaveChangesAsync(cancellationToken);

    public IDbContextTransaction Transaction => _context.Database.BeginTransaction();
}