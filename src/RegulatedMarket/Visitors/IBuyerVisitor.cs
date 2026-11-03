using RegulatedMarket.Domain.Actors;

namespace RegulatedMarket.Visitors;

public interface IBuyerVisitor
{
    void Visit(Buyer buyer);
}