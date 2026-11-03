namespace RegulatedMarket.Domain.Actors;

internal class UnSubscriber<T> : IDisposable
{
    private readonly List<IObserver<T>> _observers;
    private readonly IObserver<T> _observer;

    internal UnSubscriber
    (
        List<IObserver<T>> observers, 
        IObserver<T> observer
    )
    {
       _observers = observers;
       _observer = observer;
    }

    public void Dispose()
    {
        if (_observer != null)
        {
            _observers.Remove(_observer);
        }
    }
}