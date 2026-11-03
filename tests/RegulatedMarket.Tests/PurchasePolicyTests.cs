using RegulatedMarket.Domain.Actors;
using RegulatedMarket.Domain.Market;
using RegulatedMarket.Domain.Products;
using RegulatedMarket.Domain.Purchasing;
using RegulatedMarket.Visitors;

namespace RegulatedMarket.Tests;

public class PurchasePolicyTests
{
    [Test]
    public void Execute_ReturnsZero_WhenOffersNull()
    {
        var buyer = new Buyer("Jan", 1000m, 2, 1.0m);
        var policy = new PurchasePolicy();

        var spent = policy.Execute(buyer, null);

        Assert.That(spent, Is.EqualTo(0m));
    }

    [Test]
    public void Execute_BuysExactNecessityNeed_AtOfferPrice()
    {
        var seller = new Seller("Sklep", 0m, 10);
        var buyer = new Buyer("Jan", 1000m, necessityNeedPerTurn: 3, baseWillingness: 1.0m);
        var bread = new NecessityProduct("Chleb", 10m);
        var policy = new PurchasePolicy();

        seller.AddProduct(bread);
        seller.UpdatePrices(new InflationPriceVisitor(0m, 0m, 0m));
        seller.Produce();

        var offers = new OfferSnapshot(
            seller,
            new[] { new ProductOffer(bread, bread.CurrentPrice, seller.GetStock(bread)) });

        var spent = policy.Execute(buyer, offers);

        Assert.That(spent, Is.EqualTo(30m));
        Assert.That(buyer.Budget, Is.EqualTo(970m));
        Assert.That(seller.GetStock(bread), Is.EqualTo(7));
    }

    [Test]
    public void Execute_WithEmptyOfferList_SpendsZero()
    {
        var seller = new Seller("Sklep", 0m, 10);
        var buyer = new Buyer("Jan", 1000m, 2, 1.0m);
        var policy = new PurchasePolicy();
        var offers = new OfferSnapshot(seller, Array.Empty<ProductOffer>());

        var spent = policy.Execute(buyer, offers);

        Assert.That(spent, Is.EqualTo(0m));
        Assert.That(buyer.Budget, Is.EqualTo(1000m));
    }
}
