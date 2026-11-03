using RegulatedMarket.Domain.Actors;
using RegulatedMarket.Domain.Market;
using RegulatedMarket.Visitors;

namespace RegulatedMarket.Simulation;

public class TurnBasedMarketSimulator
{
    private readonly CentralBank _bank;
    private readonly List<Seller> _sellers;
    private readonly List<Buyer> _buyers;

    public MarketStats? LastStats { get; private set; }

    public TurnBasedMarketSimulator(
        CentralBank bank,
        IEnumerable<Seller> sellers,
        IEnumerable<Buyer> buyers)
    {
        _bank = bank;
        _sellers = sellers.ToList();
        _buyers = buyers.ToList();
    }

    public MarketStats RunTurn()
    {
        foreach (var seller in _sellers)
        {
            seller.Produce();
        }

        foreach (var seller in _sellers)
        {
            var visitor = new InflationPriceVisitor(
                seller.InflationRate,
                seller.TaxRate,
                seller.Margin);
            seller.UpdatePrices(visitor);
        }

        var decisionVisitor = new BuyerDecisionVisitor(_bank.InflationRate);
        foreach (var buyer in _buyers)
        {
            buyer.Accept(decisionVisitor);
        }

        foreach (var seller in _sellers)
        {
            seller.PublishOffers();
        }

        foreach (var buyer in _buyers)
        {
            buyer.ProcessLastOffers();
        }

        foreach (var seller in _sellers)
        {
            seller.AdjustMarginAfterSales();
        }

        decimal turnover = 0m;
        foreach (var buyer in _buyers)
        {
            turnover += buyer.SpentThisTurn;
        }

        var stats = new MarketStats(turnover, 0);
        LastStats = stats;
        _bank.AdjustFromMarket(stats);

        return stats;
    }
}
