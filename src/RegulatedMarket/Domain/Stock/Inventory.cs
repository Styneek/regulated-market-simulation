using RegulatedMarket.Domain.Products;

namespace RegulatedMarket.Domain.Stock;

public class Inventory
{
    private readonly Dictionary<IProduct, int> _stock = new();
    private readonly List<IProduct> _products = new();

    public IReadOnlyList<IProduct> Products => _products;

    public void AddProduct(IProduct product)
    {
        if (_products.Contains(product))
        {
            return;
        }

        _products.Add(product);
        _stock[product] = 0;
    }

    public void Produce(int unitsPerProduct)
    {
        foreach (var product in _products)
        {
            _stock[product] += unitsPerProduct;
        }
    }

    public int GetStock(IProduct product)
    {
        return _stock.TryGetValue(product, out var qty) ? qty : 0;
    }

    public bool TryTake(IProduct product, int quantity)
    {
        if (!_stock.TryGetValue(product, out var available) || available < quantity)
        {
            return false;
        }

        _stock[product] = available - quantity;
        return true;
    }
}
