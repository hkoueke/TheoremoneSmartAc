namespace SmartAc.Infrastructure.BackgroundJobs.Handlers;

internal abstract class AbstractHandler<T> where T : notnull
{
    public AbstractHandler<T>? Next { get; private set; }

    public void SetNextHandler(AbstractHandler<T> next) => Next = next;

    public abstract Task Handle(T item, CancellationToken cancellationToken);
}