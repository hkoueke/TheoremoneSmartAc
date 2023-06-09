﻿using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;

namespace SmartAc.Persistence.Repositories;

internal sealed class Repository<TEntity> : IRepository<TEntity> where TEntity : EntityBase
{
    private readonly DbSet<TEntity> _table;

    public Repository(SmartAcContext context)
    {
        _table = context.Set<TEntity>();
    }

    public ValueTask<TEntity?> FindByIdAsync(object id, CancellationToken cancellationToken = default) => 
        _table.FindAsync(new[] { id }, cancellationToken);

    public IQueryable<TEntity> GetQueryable(ISpecification<TEntity> specification) => 
        ApplySpecification(specification);

    public void Add(TEntity entity) => _table.Add(entity);

    public void AddRange(IEnumerable<TEntity> entities) => _table.AddRange(entities);

    public void Update(TEntity entity) => _table.Update(entity);

    public void Remove(TEntity entity) => _table.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => _table.RemoveRange(entities);

    public Task<bool> ContainsAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return ApplySpecification(specification).AnyAsync(cancellationToken);
    }

    public Task<bool> ContainsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _table.Where(predicate)
                     .AnyAsync(cancellationToken);
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _table.Where(predicate)
                     .CountAsync(cancellationToken);
    }

    public Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return ApplySpecification(specification).CountAsync(cancellationToken);
    }

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification) => 
        SpecificationEvaluator<TEntity>.GetQuery(_table.AsQueryable(), specification);
}