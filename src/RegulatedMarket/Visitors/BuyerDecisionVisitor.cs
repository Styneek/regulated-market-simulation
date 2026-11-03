using RegulatedMarket.Domain.Actors;

namespace RegulatedMarket.Visitors;

public class BuyerDecisionVisitor : IBuyerVisitor
{
    private readonly decimal _inflation;

    public BuyerDecisionVisitor(decimal inflation)
    {
        _inflation = inflation;
    }

    public void Visit(Buyer buyer)
    {
        var newWillingness = buyer.BaseWillingness / (1m + _inflation);
        buyer.SetWillingness(newWillingness);
    }
}