using RegulatedMarket.Domain.Products;
namespace RegulatedMarket.Visitors;

public interface IProductVisitor 
{
    void Visit(NecessityProduct product);
    void Visit(LuxuryProduct product);
}