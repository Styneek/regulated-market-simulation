using RegulatedMarket.Visitors;

namespace RegulatedMarket.Domain.Products;

public class NecessityProduct : ProductBase
{
    public override ProductCategory Category => ProductCategory.Necessity;

    public NecessityProduct(string name, decimal productionCost)
        : base(name, productionCost)
    {
    }

    public override void Accept(IProductVisitor visitor) => visitor.Visit(this);
}
