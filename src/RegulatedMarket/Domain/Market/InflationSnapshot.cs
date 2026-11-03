namespace RegulatedMarket.Domain.Market;

public class InflationSnapshot
{
    public decimal InflationRate { get; set; } 
    public decimal TaxRate { get; set; }

    public InflationSnapshot(decimal inflationRate, decimal taxRate)
    {
        InflationRate = inflationRate;
        TaxRate = taxRate;
    }
}