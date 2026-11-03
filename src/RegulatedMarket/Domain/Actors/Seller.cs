using RegulatedMarket.Domain.Market;
using RegulatedMarket.Domain.Products;
using RegulatedMarket.Domain.Stock;
using RegulatedMarket.Visitors;

namespace RegulatedMarket.Domain.Actors;

public class Seller : IObserver<InflationSnapshot>, IObservable<OfferSnapshot>, ISeller
{
    private readonly Inventory _inventory = new();
    private readonly List<IObserver<OfferSnapshot>> _offerObservers = new();
    private int _unitsSoldLastTurn; // do korekty marzy

    public string Name { get; }
    public decimal Margin { get; private set; }
    public int ProductionLimitPerTurn { get; }
    public decimal InflationRate { get; private set; }
    public decimal TaxRate { get; private set; }

    public Seller(string name, decimal margin, int productionLimitPerTurn)
    {
        Name = name;
        Margin = margin;
        ProductionLimitPerTurn = productionLimitPerTurn;
    }

    public void OnNext(InflationSnapshot value)
    {
        InflationRate = value.InflationRate;
        TaxRate = value.TaxRate;
    }

    public void OnError(Exception error)
    {
    }

    public void OnCompleted()
    {
    }

    public void AddProduct(IProduct product) => _inventory.AddProduct(product);

    public void Produce() => _inventory.Produce(ProductionLimitPerTurn);

    public int GetStock(IProduct product) => _inventory.GetStock(product);

    public void UpdatePrices(IProductVisitor visitor)
    {
        foreach (var product in _inventory.Products)
        {
            product.Accept(visitor);
        }
    }

    public IDisposable Subscribe(IObserver<OfferSnapshot> observer)
    {
        if (!_offerObservers.Contains(observer))
        {
            _offerObservers.Add(observer);
        }

        return new UnSubscriber<OfferSnapshot>(_offerObservers, observer);
    }

    public void PublishOffers()
    {
        _unitsSoldLastTurn = 0;
        var offers = new List<ProductOffer>();

        foreach (var product in _inventory.Products)
        {
            var qty = _inventory.GetStock(product);
            if (qty > 0)
            {
                offers.Add(new ProductOffer(product, product.CurrentPrice, qty));
            }
        }

        var snapshot = new OfferSnapshot(this, offers);
        foreach (var observer in _offerObservers)
        {
            observer.OnNext(snapshot);
        }
    }

    public bool TrySell(IProduct product, int quantity)
    {
        if (!_inventory.TryTake(product, quantity))
        {
            return false;
        }

        _unitsSoldLastTurn += quantity;
        return true;
    }

    public void SetMargin(decimal margin)
    {
        Margin = margin;
    }

    public void AdjustMarginAfterSales()
    {
        var capacity = ProductionLimitPerTurn * Math.Max(1, _inventory.Products.Count);
        if (capacity <= 0)
        {
            return;
        }

        var sellThrough = (decimal)_unitsSoldLastTurn / capacity;
        if (sellThrough >= 0.8m)
        {
            Margin += 0.01m;
        }
        else if (sellThrough <= 0.2m && Margin > 0.01m)
        {
            Margin -= 0.01m;
        }
    }
}
