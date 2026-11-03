using RegulatedMarket.Domain.Products;

namespace RegulatedMarket.Domain.Actors;

public interface ISeller
{
    string Name { get; }
    bool TrySell(IProduct product, int quantity);
}
