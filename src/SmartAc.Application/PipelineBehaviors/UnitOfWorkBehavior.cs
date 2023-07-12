using MediatR;
using Microsoft.Extensions.Logging;
using SmartAc.Application.Abstractions.Messaging;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Extensions;
using SmartAc.Domain;

namespace SmartAc.Application.PipelineBehaviors;

internal sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<HashStore> _repository;
    private readonly ILogger<UnitOfWorkBehavior<TRequest, TResponse>> _logger;

    public UnitOfWorkBehavior(
        IUnitOfWork unitOfWork,
        IRepository<HashStore> repository,
        ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICommand<TResponse>)
        {
            return await next();
        }

        var idempotent = request as IIdempotentCommand<TResponse>;

        if (idempotent is not null &&
            await _repository.ContainsAsync(hs =>
                hs.HashCode == idempotent.HashCode,
                cancellationToken))
        {
            _logger.LogInformation("Request '{@RequestName}' is idempotent and has already been executed once.", request.GetType().Name);
            return await next();
        }

        //using var transactionScope = new TransactionScope(TransactionScopeOption.Required,
        //    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted });

        await using var transaction = _unitOfWork.Transaction;

        try
        {
            var response = await next();

            _repository.Add(new HashStore
            {
                HashCode = idempotent?.HashCode ?? request.GetHashString(),
                FromCommand = request.GetType().Name
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return response;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}