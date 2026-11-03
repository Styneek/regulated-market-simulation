using RegulatedMarket.Domain.Actors;

namespace RegulatedMarket.Domain.Market;

public class OfferSnapshot
{
    public ISeller Seller { get; }
    public IReadOnlyList<ProductOffer> Offers { get; }

    public OfferSnapshot(ISeller seller, IReadOnlyList<ProductOffer> offers)
    {
        Seller = seller;
        Offers = offers;
    }
}
