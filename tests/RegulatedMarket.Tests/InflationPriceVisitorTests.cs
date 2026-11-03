using RegulatedMarket.Domain.Products;
using RegulatedMarket.Visitors;

namespace RegulatedMarket.Tests;

public class InflationPriceVisitorTests
{
    [Test]
    public void Visit_Necessity_SetsPriceFromFormula()
    {
        var product = new NecessityProduct("Chleb", 100m);
        var visitor = new InflationPriceVisitor(0.1m, 0.1m, 0.1m);

        product.Accept(visitor);

        Assert.That(product.CurrentPrice, Is.EqualTo(133.1m));
    }

    [Test]
    public void Visit_Luxury_SetsPriceFromSameFormula()
    {
        var product = new LuxuryProduct("Zegarek", 100m);
        var visitor = new InflationPriceVisitor(0.1m, 0.1m, 0.1m);

        product.Accept(visitor);

        Assert.That(product.CurrentPrice, Is.EqualTo(133.1m));
    }

    [Test]
    public void Visit_WithZeroRates_KeepsProductionCost()
    {
        var product = new NecessityProduct("Chleb", 42m);
        var visitor = new InflationPriceVisitor(0m, 0m, 0m);

        product.Accept(visitor);

        Assert.That(product.CurrentPrice, Is.EqualTo(42m));
    }
}
