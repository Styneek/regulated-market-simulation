using RegulatedMarket.Domain.Products;
using RegulatedMarket.Domain.Stock;

namespace RegulatedMarket.Tests;

public class InventoryTests
{
    [Test]
    public void Produce_AddsUnitsPerProduct()
    {
        var inventory = new Inventory();
        var bread = new NecessityProduct("Chleb", 10m);
        var watch = new LuxuryProduct("Zegarek", 50m);

        inventory.AddProduct(bread);
        inventory.AddProduct(watch);
        inventory.Produce(5);

        Assert.That(inventory.GetStock(bread), Is.EqualTo(5));
        Assert.That(inventory.GetStock(watch), Is.EqualTo(5));
    }

    [Test]
    public void TryTake_ReducesStock_WhenEnoughAvailable()
    {
        var inventory = new Inventory();
        var bread = new NecessityProduct("Chleb", 10m);
        inventory.AddProduct(bread);
        inventory.Produce(10);

        var ok = inventory.TryTake(bread, 3);

        Assert.That(ok, Is.True);
        Assert.That(inventory.GetStock(bread), Is.EqualTo(7));
    }

    [Test]
    public void TryTake_Fails_WhenNotEnoughStock()
    {
        var inventory = new Inventory();
        var bread = new NecessityProduct("Chleb", 10m);
        inventory.AddProduct(bread);
        inventory.Produce(2);

        var ok = inventory.TryTake(bread, 3);

        Assert.That(ok, Is.False);
        Assert.That(inventory.GetStock(bread), Is.EqualTo(2));
    }

    [Test]
    public void AddProduct_DoesNotDuplicate()
    {
        var inventory = new Inventory();
        var bread = new NecessityProduct("Chleb", 10m);

        inventory.AddProduct(bread);
        inventory.AddProduct(bread);

        Assert.That(inventory.Products.Count, Is.EqualTo(1));
    }

    [Test]
    public void TryTake_Fails_WhenProductNotInInventory()
    {
        var inventory = new Inventory();
        var bread = new NecessityProduct("Chleb", 10m);

        var ok = inventory.TryTake(bread, 1);

        Assert.That(ok, Is.False);
        Assert.That(inventory.GetStock(bread), Is.EqualTo(0));
    }

    [Test]
    public void Produce_Zero_DoesNotChangeStock()
    {
        var inventory = new Inventory();
        var bread = new NecessityProduct("Chleb", 10m);
        inventory.AddProduct(bread);
        inventory.Produce(4);

        inventory.Produce(0);

        Assert.That(inventory.GetStock(bread), Is.EqualTo(4));
    }
}
