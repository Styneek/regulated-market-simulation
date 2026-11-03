using RegulatedMarket.Visitors;

namespace RegulatedMarket.Domain.Products;

public interface IProduct
{
    string Name { get; }
    decimal ProductionCost { get; }
    decimal CurrentPrice { get; set; }
    ProductCategory Category { get; }

    void Accept(IProductVisitor visitor);
}
