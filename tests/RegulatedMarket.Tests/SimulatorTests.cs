using RegulatedMarket.Domain.Actors;
using RegulatedMarket.Domain.Products;
using RegulatedMarket.Simulation;

namespace RegulatedMarket.Tests;

public class SimulatorTests
{
    [Test]
    public void RunTurn_ProducesTurnover_AndBankReacts()
    {
        var bank = new CentralBank(0.1m, 0.1m, targetTaxRevenue: 50m, maxInflationStep: 0.05m);
        var seller = new Seller("Sklep", 0.1m, 10);
        var buyer = new Buyer("Jan", 5000m, 3, 1.0m);
        var product = new NecessityProduct("Chleb", 10m);

        bank.Subscribe(seller);
        bank.Subscribe(buyer);
        seller.Subscribe(buyer);
        seller.AddProduct(product);
        bank.SetInflationRate(0.1m);

        var inflationBefore = bank.InflationRate;
        var sim = new TurnBasedMarketSimulator(bank, new[] { seller }, new[] { buyer });
        var stats = sim.RunTurn();

        Assert.That(stats.TotalTurnover, Is.GreaterThan(0m));
        Assert.That(bank.LastTaxRevenue, Is.EqualTo(inflationBefore * stats.TotalTurnover));
        Assert.That(bank.InflationRate, Is.Not.EqualTo(inflationBefore));
    }

    [Test]
    public void RunTurn_PropagatesBankInflation_ToSellerAndBuyer()
    {
        var bank = new CentralBank(0.05m, 0.1m, targetTaxRevenue: 50m);
        var seller = new Seller("Sklep", 0.1m, 10);
        var buyer = new Buyer("Jan", 5000m, 2, 1.0m);
        var product = new NecessityProduct("Chleb", 10m);

        bank.Subscribe(seller);
        bank.Subscribe(buyer);
        seller.Subscribe(buyer);
        seller.AddProduct(product);
        bank.SetInflationRate(0.12m);

        var sim = new TurnBasedMarketSimulator(bank, new[] { seller }, new[] { buyer });
        sim.RunTurn();

        Assert.That(seller.InflationRate, Is.EqualTo(bank.InflationRate));
        Assert.That(buyer.InflationRate, Is.EqualTo(bank.InflationRate));
        Assert.That(buyer.Willingness, Is.EqualTo(1m / 1.12m));
    }

    [Test]
    public void RunTurn_WithNoBuyers_HasZeroTurnover_AndBankStillAdjusts()
    {
        var bank = new CentralBank(0.10m, 0.1m, targetTaxRevenue: 50m, maxInflationStep: 0.05m);
        var seller = new Seller("Sklep", 0.1m, 10);
        seller.AddProduct(new NecessityProduct("Chleb", 10m));
        bank.Subscribe(seller);
        bank.SetInflationRate(0.10m);

        var sim = new TurnBasedMarketSimulator(bank, new[] { seller }, Array.Empty<Buyer>());
        var stats = sim.RunTurn();

        Assert.That(stats.TotalTurnover, Is.EqualTo(0m));
        Assert.That(bank.LastTaxRevenue, Is.EqualTo(0m));
        Assert.That(bank.InflationRate, Is.EqualTo(0.15m));
    }
}
