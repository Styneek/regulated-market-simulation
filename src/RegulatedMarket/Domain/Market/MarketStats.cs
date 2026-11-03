namespace RegulatedMarket.Domain.Market;

public class MarketStats
{
    public decimal TotalTurnover { get; }
    public int UnitsSold { get; }

    public MarketStats(decimal totalTurnover, int unitsSold)
    {
        TotalTurnover = totalTurnover;
        UnitsSold = unitsSold;
    }
}