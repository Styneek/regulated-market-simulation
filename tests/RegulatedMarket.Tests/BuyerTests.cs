using RegulatedMarket.Domain.Actors;
using RegulatedMarket.Domain.Products;
using RegulatedMarket.Visitors;

namespace RegulatedMarket.Tests;

public class BuyerTests
{
    [Test]
    public void BuyerDecisionVisitor_LowersWillingness()
    {
        var buyer = new Buyer("Jan", 1000m, 2, 1.0m);
        buyer.Accept(new BuyerDecisionVisitor(0.25m));

        Assert.That(buyer.Willingness, Is.EqualTo(0.8m));
    }

    [Test]
    public void ProcessLastOffers_BuysNecessity_UpToNeed()
    {
        var seller = new Seller("Sklep", 0.0m, 10);
        var buyer = new Buyer("Jan", 1000m, 2, 1.0m);
        var product = new NecessityProduct("Chleb", 10m);

        seller.Subscribe(buyer);
        seller.AddProduct(product);
        seller.UpdatePrices(new InflationPriceVisitor(0m, 0m, 0m));
        seller.Produce();
        seller.PublishOffers();
        buyer.ProcessLastOffers();

        Assert.That(buyer.SpentThisTurn, Is.EqualTo(20m)); 
        Assert.That(buyer.Budget, Is.EqualTo(980m));
        Assert.That(seller.GetStock(product), Is.EqualTo(8));
    }

    [Test]
    public void ProcessLastOffers_BuysLuxury_OnlyAfterNecessityNeedMet()
    {
        var seller = new Seller("Sklep", 0.0m, 10);
        var buyer = new Buyer("Jan", 1000m, necessityNeedPerTurn: 1, baseWillingness: 1.0m);
        var bread = new NecessityProduct("Chleb", 10m);
        var watch = new LuxuryProduct("Zegarek", 50m);

        seller.Subscribe(buyer);
        seller.AddProduct(bread);
        seller.AddProduct(watch);
        seller.UpdatePrices(new InflationPriceVisitor(0m, 0m, 0m));
        seller.Produce();
        seller.PublishOffers();
        buyer.ProcessLastOffers();

        Assert.That(buyer.SpentThisTurn, Is.EqualTo(60m));
        Assert.That(seller.GetStock(bread), Is.EqualTo(9));
        Assert.That(seller.GetStock(watch), Is.EqualTo(9));
    }

    [Test]
    public void ProcessLastOffers_SkipsLuxury_WhenNecessityNeedUnmet()
    {
        var seller = new Seller("Sklep", 0.0m, 10);
        // need 2, but no necessity in offers — only luxury
        var buyer = new Buyer("Jan", 1000m, necessityNeedPerTurn: 2, baseWillingness: 1.0m);
        var watch = new LuxuryProduct("Zegarek", 50m);

        seller.Subscribe(buyer);
        seller.AddProduct(watch);
        seller.UpdatePrices(new InflationPriceVisitor(0m, 0m, 0m));
        seller.Produce();
        seller.PublishOffers();
        buyer.ProcessLastOffers();

        Assert.That(buyer.SpentThisTurn, Is.EqualTo(0m));
        Assert.That(seller.GetStock(watch), Is.EqualTo(10));
    }

    [Test]
    public void ProcessLastOffers_DoesNotBuy_WhenCannotAfford()
    {
        var seller = new Seller("Sklep", 0.0m, 10);
        var buyer = new Buyer("Jan", budget: 5m, necessityNeedPerTurn: 2, baseWillingness: 1.0m);
        var bread = new NecessityProduct("Chleb", 10m);

        seller.Subscribe(buyer);
        seller.AddProduct(bread);
        seller.UpdatePrices(new InflationPriceVisitor(0m, 0m, 0m));
        seller.Produce();
        seller.PublishOffers();
        buyer.ProcessLastOffers();

        Assert.That(buyer.SpentThisTurn, Is.EqualTo(0m));
        Assert.That(buyer.Budget, Is.EqualTo(5m));
        Assert.That(seller.GetStock(bread), Is.EqualTo(10));
    }

    [Test]
    public void CanAfford_RespectsWillingnessCap()
    {
        var buyer = new Buyer("Jan", budget: 1000m, necessityNeedPerTurn: 1, baseWillingness: 1.0m);
        buyer.SetWillingness(0.1m); 

        Assert.That(buyer.CanAfford(100m), Is.True);
        Assert.That(buyer.CanAfford(100.01m), Is.False);
    }

    [Test]
    public void ProcessLastOffers_BuysPartialNecessity_WhenBudgetCoversOnlyOne()
    {
        var seller = new Seller("Sklep", 0.0m, 10);
        var buyer = new Buyer("Jan", budget: 15m, necessityNeedPerTurn: 2, baseWillingness: 1.0m);
        var bread = new NecessityProduct("Chleb", 10m);

        seller.Subscribe(buyer);
        seller.AddProduct(bread);
        seller.UpdatePrices(new InflationPriceVisitor(0m, 0m, 0m));
        seller.Produce();
        seller.PublishOffers();
        buyer.ProcessLastOffers();

        Assert.That(buyer.SpentThisTurn, Is.EqualTo(10m));
        Assert.That(buyer.Budget, Is.EqualTo(5m));
        Assert.That(seller.GetStock(bread), Is.EqualTo(9));
    }

    [Test]
    public void ProcessLastOffers_WithNoOffersReceived_SpendsZero()
    {
        var buyer = new Buyer("Jan", 1000m, 2, 1.0m);

        buyer.ProcessLastOffers();

        Assert.That(buyer.SpentThisTurn, Is.EqualTo(0m));
        Assert.That(buyer.Budget, Is.EqualTo(1000m));
    }
}
