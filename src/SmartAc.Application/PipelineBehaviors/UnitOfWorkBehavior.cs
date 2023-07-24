using MediatR;
using SmartAc.Application.Abstractions.Messaging;
using SmartAc.Application.Abstractions.Repositories;

namespace SmartAc.Application.PipelineBehaviors;

internal sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICommand<TResponse>)
        {
            return await next();
        }

        //using var transactionScope = new TransactionScope(TransactionScopeOption.Required,
        //    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted });

        await using var transaction = _unitOfWork.Transaction;

        try
        {
            var response = await next();

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