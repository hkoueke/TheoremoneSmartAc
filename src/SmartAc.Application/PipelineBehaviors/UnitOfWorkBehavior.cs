using MediatR;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Helpers;
using SmartAc.Domain;

namespace SmartAc.Application.PipelineBehaviors;

internal sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<HashStore> _repository;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork, IRepository<HashStore> repository)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (IsNotCommand())
            return await next();

        //using var transactionScope = new TransactionScope(TransactionScopeOption.Required,
        //    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted });

        await using var transaction = _unitOfWork.Transaction;

        try
        {
            var response = await next();

            var hashCode = request.GetHexString();

            if (!await _repository.ContainsAsync(hs => hs.HashCode == hashCode, cancellationToken))
            {
                _repository.Add(new HashStore
                {
                    HashCode = hashCode,
                    FromCommand = request.GetType().Name
                });
            }

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

    private static bool IsNotCommand() => !typeof(TRequest).Name.EndsWith("Command");
}