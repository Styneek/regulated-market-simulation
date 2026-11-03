using RegulatedMarket.Domain.Products;

namespace RegulatedMarket.Domain.Market;

public class ProductOffer
{
    public IProduct Product { get; }
    public decimal Price { get; }
    public int Quantity { get; }

    public ProductOffer(IProduct product, decimal price, int quantity)
    {
        Product = product;
        Price = price;
        Quantity = quantity;
    }
}
