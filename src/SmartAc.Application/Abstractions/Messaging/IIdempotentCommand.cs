namespace SmartAc.Application.Abstractions.Messaging;

internal interface IIdempotentCommand<out TResponse> : ICommand<TResponse>
{
    string HashCode { get; }
}