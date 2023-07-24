using MediatR;
using SmartAc.Application.Abstractions.Messaging;
using SmartAc.Application.Abstractions.Services;

namespace SmartAc.Application.PipelineBehaviors;

internal sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IIdempotentCommand<TResponse>
{
    private readonly IIdempotentService _idempotentService;

    public IdempotencyBehavior(IIdempotentService idempotentService)
    {
        _idempotentService = idempotentService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (await _idempotentService.RequestExistsAsync(request.HashString, cancellationToken))
        {
            return default!;
        }

        await _idempotentService.CreateRequestEntryAsync(request.HashString, typeof(TRequest).Name, cancellationToken);

        return await next();
    }
}