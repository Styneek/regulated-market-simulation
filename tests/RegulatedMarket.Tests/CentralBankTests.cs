using RegulatedMarket.Domain.Actors;
using RegulatedMarket.Domain.Market;

namespace RegulatedMarket.Tests;

public class CentralBankTests
{
    [Test]
    public void SetInflationRate_NotifiesSubscriber()
    {
        var bank = new CentralBank(0.05m, 0.1m);
        var observer = new InflationTestObserver();

        bank.Subscribe(observer);
        bank.SetInflationRate(0.12m);

        Assert.That(observer.Last, Is.Not.Null);
        Assert.That(observer.Last!.InflationRate, Is.EqualTo(0.12m));
        Assert.That(observer.Last.TaxRate, Is.EqualTo(0.1m));
    }

    [Test]
    public void Dispose_StopsNotifications()
    {
        var bank = new CentralBank(0.05m, 0.1m);
        var observer = new InflationTestObserver();
        var subscription = bank.Subscribe(observer);

        bank.SetInflationRate(0.08m);
        Assert.That(observer.Last!.InflationRate, Is.EqualTo(0.08m));

        subscription.Dispose();
        bank.SetInflationRate(0.20m);

        Assert.That(observer.Last.InflationRate, Is.EqualTo(0.08m));
        Assert.That(observer.NotificationCount, Is.EqualTo(1));
    }

    [Test]
    public void AdjustFromMarket_SetsLastTaxRevenue_AsInflationTimesTurnover()
    {
        var bank = new CentralBank(0.10m, 0.1m, targetTaxRevenue: 50m, maxInflationStep: 1m);

        bank.AdjustFromMarket(new MarketStats(totalTurnover: 200m, unitsSold: 0));

        Assert.That(bank.LastTaxRevenue, Is.EqualTo(20m));
    }

    [Test]
    public void AdjustFromMarket_RaisesInflation_TowardDesired_CappedByMaxStep()
    {
        var bank = new CentralBank(0.10m, 0.1m, targetTaxRevenue: 50m, maxInflationStep: 0.05m);

        bank.AdjustFromMarket(new MarketStats(100m, 0));

        Assert.That(bank.InflationRate, Is.EqualTo(0.15m));
        Assert.That(bank.LastTaxRevenue, Is.EqualTo(10m)); // 0.10 * 100
    }

    [Test]
    public void AdjustFromMarket_LowersInflation_TowardDesired_CappedByMaxStep()
    {
        var bank = new CentralBank(0.20m, 0.1m, targetTaxRevenue: 10m, maxInflationStep: 0.05m);

        bank.AdjustFromMarket(new MarketStats(1000m, 0));

        Assert.That(bank.InflationRate, Is.EqualTo(0.15m));
        Assert.That(bank.LastTaxRevenue, Is.EqualTo(200m)); // 0.20 * 1000
    }

    [Test]
    public void AdjustFromMarket_AppliesDesiredExactly_WhenWithinMaxStep()
    {
        var bank = new CentralBank(0.10m, 0.1m, targetTaxRevenue: 30m, maxInflationStep: 0.05m);

        bank.AdjustFromMarket(new MarketStats(200m, 0));

        Assert.That(bank.InflationRate, Is.EqualTo(0.15m));
    }

    [Test]
    public void AdjustFromMarket_DoesNotGoBelowFloor()
    {
        var bank = new CentralBank(0.02m, 0.1m, targetTaxRevenue: 1m, maxInflationStep: 0.05m);

        bank.AdjustFromMarket(new MarketStats(1000m, 0));

        Assert.That(bank.InflationRate, Is.EqualTo(0.01m));
    }

    [Test]
    public void AdjustFromMarket_NotifiesObservers_AfterChange()
    {
        var bank = new CentralBank(0.10m, 0.1m, targetTaxRevenue: 50m, maxInflationStep: 0.05m);
        var observer = new InflationTestObserver();
        bank.Subscribe(observer);

        bank.AdjustFromMarket(new MarketStats(100m, 0));

        Assert.That(observer.Last, Is.Not.Null);
        Assert.That(observer.Last!.InflationRate, Is.EqualTo(0.15m));
    }

    [Test]
    public void AdjustFromMarket_WithZeroTurnover_UsesEpsilon_AndDoesNotThrow()
    {
        var bank = new CentralBank(0.10m, 0.1m, targetTaxRevenue: 50m, maxInflationStep: 0.05m);

        Assert.DoesNotThrow(() => bank.AdjustFromMarket(new MarketStats(0m, 0)));

        Assert.That(bank.LastTaxRevenue, Is.EqualTo(0m)); // 0.10 * 0
        Assert.That(bank.InflationRate, Is.EqualTo(0.15m));
    }
}

file class InflationTestObserver : IObserver<InflationSnapshot>
{
    public InflationSnapshot? Last { get; private set; }
    public int NotificationCount { get; private set; }

    public void OnNext(InflationSnapshot value)
    {
        Last = value;
        NotificationCount++;
    }

    public void OnError(Exception error)
    {
    }

    public void OnCompleted()
    {
    }
}
