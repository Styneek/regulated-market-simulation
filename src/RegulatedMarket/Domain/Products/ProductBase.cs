using RegulatedMarket.Visitors;

namespace RegulatedMarket.Domain.Products;

public abstract class ProductBase : IProduct
{
    public string Name { get; }
    public decimal ProductionCost { get; }
    public decimal CurrentPrice { get; set; }
    public abstract ProductCategory Category { get; }

    protected ProductBase(string name, decimal productionCost)
    {
        Name = name;
        ProductionCost = productionCost;
        CurrentPrice = productionCost;
    }

    public abstract void Accept(IProductVisitor visitor);
}
