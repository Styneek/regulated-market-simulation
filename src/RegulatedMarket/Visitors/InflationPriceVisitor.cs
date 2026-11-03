using RegulatedMarket.Domain.Products;
namespace RegulatedMarket.Visitors;

public class InflationPriceVisitor : IProductVisitor
{
    private readonly decimal _inflation;
    private readonly decimal _tax;
    private readonly decimal _margin;

    public InflationPriceVisitor(decimal inflation, decimal tax, decimal margin)
    {
        _inflation = inflation;
        _tax = tax;
        _margin = margin;
    }

    public void Visit(NecessityProduct product)
    {
        product.CurrentPrice = Calculate(product.ProductionCost);
    }
    public void Visit(LuxuryProduct product)
    {
        product.CurrentPrice = Calculate(product.ProductionCost);
    }

    private decimal Calculate(decimal cost)
    {
        return cost * (1 + _inflation) * (1 + _tax) * (1 + _margin);
    }
}