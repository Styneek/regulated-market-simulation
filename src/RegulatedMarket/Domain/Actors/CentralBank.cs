using RegulatedMarket.Domain.Market;

namespace RegulatedMarket.Domain.Actors;

public class CentralBank : IObservable<InflationSnapshot>
{
    private readonly List<IObserver<InflationSnapshot>> _observers = new();
    private readonly decimal _maxInflationStep;

    public decimal InflationRate { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TargetTaxRevenue { get; }
    public decimal LastTaxRevenue { get; private set; }

    public CentralBank(
        decimal initialInflation,
        decimal taxRate,
        decimal targetTaxRevenue = 10m,
        decimal maxInflationStep = 0.05m)
    {
        InflationRate = initialInflation;
        TaxRate = taxRate;
        TargetTaxRevenue = targetTaxRevenue;
        _maxInflationStep = maxInflationStep;
    }

    public IDisposable Subscribe(IObserver<InflationSnapshot> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }

        return new UnSubscriber<InflationSnapshot>(_observers, observer);
    }

    public void SetInflationRate(decimal inflationRate)
    {
        InflationRate = inflationRate;
        NotifyObservers();
    }

    private void NotifyObservers()
    {
        var snapshot = new InflationSnapshot(InflationRate, TaxRate);

        foreach (var observer in _observers)
        {
            observer.OnNext(snapshot);
        }
    }

    public void AdjustFromMarket(MarketStats stats)
    {
        const decimal epsilon = 0.0001m;

        LastTaxRevenue = InflationRate * stats.TotalTurnover;

        var turnover = stats.TotalTurnover;
        if (turnover < epsilon)
        {
            turnover = epsilon;
        }

        //cel np. cel 50 obrót 100 desired = 0.5
        var desired = TargetTaxRevenue / turnover;


        var delta = desired - InflationRate;
        if (delta > _maxInflationStep)
        {
            delta = _maxInflationStep;
        }

        if (delta < -_maxInflationStep)
        {
            delta = -_maxInflationStep;
        }

        InflationRate += delta;

        if (InflationRate < 0.01m)
        {
            InflationRate = 0.01m;
        }

        NotifyObservers();
    }
}
