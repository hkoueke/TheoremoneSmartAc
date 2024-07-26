namespace SmartAc.Infrastructure.Alerts.Abstractions;

internal interface IHandler<T>
{
    public IHandler<T> SetNext(IHandler<T> handler);
    public void Handle(T item);
}