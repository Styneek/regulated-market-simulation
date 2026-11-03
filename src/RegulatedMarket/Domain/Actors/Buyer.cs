using RegulatedMarket.Domain.Market;
using RegulatedMarket.Domain.Purchasing;
using RegulatedMarket.Visitors;

namespace RegulatedMarket.Domain.Actors;

public class Buyer : IObserver<InflationSnapshot>, IObserver<OfferSnapshot>
{
    private readonly PurchasePolicy _purchasePolicy;
    private OfferSnapshot? _lastOffers;

    public string Name { get; }
    public decimal Budget { get; private set; }
    public int NecessityNeedPerTurn { get; }
    public decimal BaseWillingness { get; }
    public decimal Willingness { get; private set; }
    public decimal InflationRate { get; private set; }
    public decimal SpentThisTurn { get; private set; }

    public Buyer(
        string name,
        decimal budget,
        int necessityNeedPerTurn,
        decimal baseWillingness,
        PurchasePolicy? purchasePolicy = null)
    {
        Name = name;
        Budget = budget;
        NecessityNeedPerTurn = necessityNeedPerTurn;
        BaseWillingness = baseWillingness;
        Willingness = baseWillingness;
        _purchasePolicy = purchasePolicy ?? new PurchasePolicy();
    }
    
    public void OnNext(InflationSnapshot value)
    {
        InflationRate = value.InflationRate;
    }

    public void OnNext(OfferSnapshot value)
    {
        _lastOffers = value;
    }

    public void OnError(Exception error)
    {
    }

    public void OnCompleted()
    {
    }

    public void ProcessLastOffers()
    {
        SpentThisTurn = _purchasePolicy.Execute(this, _lastOffers);
    }

    public bool CanAfford(decimal price)
    {
        return price <= Budget && price <= Budget * Willingness;
    }

    public void Pay(decimal amount)
    {
        Budget -= amount;
    }

    public void SetWillingness(decimal value)
    {
        Willingness = value;
    }

    public void Accept(IBuyerVisitor visitor) => visitor.Visit(this);
}
