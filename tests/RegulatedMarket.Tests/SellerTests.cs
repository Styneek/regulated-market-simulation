using RegulatedMarket.Domain.Actors;
using RegulatedMarket.Domain.Market;
using RegulatedMarket.Domain.Products;
using RegulatedMarket.Visitors;

namespace RegulatedMarket.Tests;

public class SellerTests
{
    [Test]
    public void BankInflation_ThenPublishOffers_NotifiesObserver()
    {
        var bank = new CentralBank(0.05m, 0.1m);
        var seller = new Seller("Piekarnia", 0.1m, 5);
        var offerObserver = new OfferTestObserver();

        bank.Subscribe(seller);
        seller.Subscribe(offerObserver);

        var chleb = new NecessityProduct("Chleb", 100m);
        seller.AddProduct(chleb);

        bank.SetInflationRate(0.1m);
        seller.UpdatePrices(new InflationPriceVisitor(seller.InflationRate, seller.TaxRate, seller.Margin));
        seller.Produce();
        seller.PublishOffers();

        Assert.That(seller.InflationRate, Is.EqualTo(0.1m));
        Assert.That(offerObserver.Last, Is.Not.Null);
        Assert.That(offerObserver.Last!.Offers.Count, Is.EqualTo(1));
        Assert.That(offerObserver.Last.Offers[0].Quantity, Is.EqualTo(5));
        // 100 * 1.1 * 1.1 * 1.1 = 133.1
        Assert.That(offerObserver.Last.Offers[0].Price, Is.EqualTo(133.1m));
    }

    [Test]
    public void TrySell_DecreasesStock_AndFailsWhenEmpty()
    {
        var seller = new Seller("Sklep", 0.1m, 5);
        var bread = new NecessityProduct("Chleb", 10m);
        seller.AddProduct(bread);
        seller.Produce();

        Assert.That(seller.TrySell(bread, 2), Is.True);
        Assert.That(seller.GetStock(bread), Is.EqualTo(3));

        Assert.That(seller.TrySell(bread, 10), Is.False);
        Assert.That(seller.GetStock(bread), Is.EqualTo(3));
    }

    [Test]
    public void SetMargin_UpdatesMargin()
    {
        var seller = new Seller("Sklep", 0.10m, 5);

        seller.SetMargin(0.35m);

        Assert.That(seller.Margin, Is.EqualTo(0.35m));
    }

    [Test]
    public void AdjustMarginAfterSales_Increases_WhenSellThroughHigh()
    {
        // capacity = productionLimit * products = 10 * 1 = 10; sell 8 → 0.8 → margin +0.01
        var seller = new Seller("Sklep", 0.10m, 10);
        var bread = new NecessityProduct("Chleb", 10m);
        seller.AddProduct(bread);
        seller.Produce();
        seller.PublishOffers();

        for (var i = 0; i < 8; i++)
        {
            Assert.That(seller.TrySell(bread, 1), Is.True);
        }

        seller.AdjustMarginAfterSales();

        Assert.That(seller.Margin, Is.EqualTo(0.11m));
    }

    [Test]
    public void AdjustMarginAfterSales_Decreases_WhenSellThroughLow()
    {
        // capacity = 10; sell 1 → 0.1 ≤ 0.2 → margin -0.01
        var seller = new Seller("Sklep", 0.10m, 10);
        var bread = new NecessityProduct("Chleb", 10m);
        seller.AddProduct(bread);
        seller.Produce();
        seller.PublishOffers();

        Assert.That(seller.TrySell(bread, 1), Is.True);

        seller.AdjustMarginAfterSales();

        Assert.That(seller.Margin, Is.EqualTo(0.09m));
    }

    [Test]
    public void AdjustMarginAfterSales_Decreases_WhenNoSales()
    {
        var seller = new Seller("Sklep", 0.10m, 10);
        var bread = new NecessityProduct("Chleb", 10m);
        seller.AddProduct(bread);
        seller.Produce();
        seller.PublishOffers();
        seller.AdjustMarginAfterSales();

        Assert.That(seller.Margin, Is.EqualTo(0.09m));
    }

    [Test]
    public void AdjustMarginAfterSales_DoesNotGoBelowFloor_WhenAlreadyAtMinimum()
    {
        var seller = new Seller("Sklep", 0.01m, 10);
        var bread = new NecessityProduct("Chleb", 10m);
        seller.AddProduct(bread);
        seller.Produce();
        seller.PublishOffers();
        seller.AdjustMarginAfterSales();

        Assert.That(seller.Margin, Is.EqualTo(0.01m));
    }

    [Test]
    public void PublishOffers_WithZeroStock_SendsEmptyOfferList()
    {
        var seller = new Seller("Sklep", 0.1m, 10);
        var observer = new OfferTestObserver();
        var bread = new NecessityProduct("Chleb", 10m);

        seller.Subscribe(observer);
        seller.AddProduct(bread);
        seller.UpdatePrices(new InflationPriceVisitor(0m, 0m, 0m));
        seller.PublishOffers();

        Assert.That(observer.Last, Is.Not.Null);
        Assert.That(observer.Last!.Offers, Is.Empty);
    }
}

file class OfferTestObserver : IObserver<OfferSnapshot>
{
    public OfferSnapshot? Last { get; private set; }
    public void OnNext(OfferSnapshot value) => Last = value;
    public void OnError(Exception error) { }
    public void OnCompleted() { }
}
