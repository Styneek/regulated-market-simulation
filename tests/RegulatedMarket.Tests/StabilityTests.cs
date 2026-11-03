using RegulatedMarket.Domain.Actors;
using RegulatedMarket.Domain.Products;
using RegulatedMarket.Simulation;

namespace RegulatedMarket.Tests;

public class StabilityTests
{
    private static (CentralBank bank, Seller seller, Buyer buyer, TurnBasedMarketSimulator sim)
        CreateMarket(
            decimal initialInflation = 0.10m,
            decimal targetTaxRevenue = 40m,
            decimal margin = 0.10m,
            decimal budget = 20_000m)
    {
        var bank = new CentralBank(initialInflation, 0.10m, targetTaxRevenue: targetTaxRevenue);
        var seller = new Seller("Sklep", margin, productionLimitPerTurn: 10);
        var buyer = new Buyer("Jan", budget, necessityNeedPerTurn: 3, baseWillingness: 1.0m);
        var product = new NecessityProduct("Chleb", 10m);

        bank.Subscribe(seller);
        bank.Subscribe(buyer);
        seller.Subscribe(buyer);
        seller.AddProduct(product);
        bank.SetInflationRate(initialInflation);

        var sim = new TurnBasedMarketSimulator(bank, new[] { seller }, new[] { buyer });
        return (bank, seller, buyer, sim);
    }

    private static decimal Abs(decimal value) => value < 0 ? -value : value;

    private static bool IsNearTarget(CentralBank bank, decimal absoluteTolerance = 12m)
    {
        return Abs(bank.LastTaxRevenue - bank.TargetTaxRevenue) < absoluteTolerance;
    }

    private static bool HasSmallRecentInflationDeltas(IReadOnlyList<decimal> inflationHistory, int lastTurns = 5, decimal maxDelta = 0.02m)
    {
        if (inflationHistory.Count < lastTurns + 1)
        {
            return false;
        }

        for (var i = inflationHistory.Count - lastTurns; i < inflationHistory.Count; i++)
        {
            var delta = Abs(inflationHistory[i] - inflationHistory[i - 1]);
            if (delta >= maxDelta)
            {
                return false;
            }
        }

        return true;
    }

    [Test]
    public void Market_Reaches_Stable_Inflation_WithoutShock()
    {
        var (bank, _, _, sim) = CreateMarket();
        var inflationHistory = new List<decimal>();
        var revenueHistory = new List<decimal>();

        for (var i = 0; i < 25; i++)
        {
            sim.RunTurn();
            inflationHistory.Add(bank.InflationRate);
            revenueHistory.Add(bank.LastTaxRevenue);
        }

        Assert.That(
            IsNearTarget(bank),
            Is.True,
            $"Expected LastTaxRevenue near TargetTaxRevenue. " +
            $"LastTaxRevenue={bank.LastTaxRevenue}, Target={bank.TargetTaxRevenue}, " +
            $"Inflation={bank.InflationRate}");
        Assert.That(
            HasSmallRecentInflationDeltas(inflationHistory),
            Is.True,
            "Expected small inflation deltas in the last turns (settled market).");
    }

    [Test]
    public void MarginShock_Then_System_Restabilizes()
    {
        var (bank, seller, _, sim) = CreateMarket();
        var inflationHistory = new List<decimal>();

        for (var i = 0; i < 10; i++)
        {
            sim.RunTurn();
            inflationHistory.Add(bank.InflationRate);
        }

        seller.SetMargin(0.50m);

        for (var i = 0; i < 20; i++)
        {
            sim.RunTurn();
            inflationHistory.Add(bank.InflationRate);
        }

        Assert.That(
            IsNearTarget(bank) || HasSmallRecentInflationDeltas(inflationHistory),
            Is.True,
            $"After margin shock expected restabilization. " +
            $"LastTaxRevenue={bank.LastTaxRevenue}, Target={bank.TargetTaxRevenue}, " +
            $"Inflation={bank.InflationRate}, Margin={seller.Margin}");
    }

    [Test]
    public void InflationImpulse_Then_System_Restabilizes()
    {
        var (bank, seller, buyer, sim) = CreateMarket();
        var inflationHistory = new List<decimal>();

        for (var i = 0; i < 10; i++)
        {
            sim.RunTurn();
            inflationHistory.Add(bank.InflationRate);
        }

        const decimal impulse = 0.40m;
        bank.SetInflationRate(impulse);

        Assert.That(seller.InflationRate, Is.EqualTo(impulse));
        Assert.That(buyer.InflationRate, Is.EqualTo(impulse));

        for (var i = 0; i < 20; i++)
        {
            sim.RunTurn();
            inflationHistory.Add(bank.InflationRate);
        }

        Assert.That(bank.InflationRate, Is.Not.EqualTo(impulse));
        Assert.That(
            IsNearTarget(bank) || HasSmallRecentInflationDeltas(inflationHistory),
            Is.True,
            $"After inflation impulse expected restabilization. " +
            $"LastTaxRevenue={bank.LastTaxRevenue}, Target={bank.TargetTaxRevenue}, " +
            $"Inflation={bank.InflationRate}");
    }
}
