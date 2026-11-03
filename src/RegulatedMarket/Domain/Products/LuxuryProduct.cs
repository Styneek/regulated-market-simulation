using RegulatedMarket.Visitors;

namespace RegulatedMarket.Domain.Products;

public class LuxuryProduct : ProductBase
{
    public override ProductCategory Category => ProductCategory.Luxury;

    public LuxuryProduct(string name, decimal productionCost)
        : base(name, productionCost)
    {
    }

    public override void Accept(IProductVisitor visitor) => visitor.Visit(this);
}
