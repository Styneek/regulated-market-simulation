using RegulatedMarket.Domain.Actors;
using RegulatedMarket.Domain.Market;
using RegulatedMarket.Domain.Products;

namespace RegulatedMarket.Domain.Purchasing;

public class PurchasePolicy
{
    public decimal Execute(Buyer buyer, OfferSnapshot? offers)
    {
        decimal spent = 0m;
        if (offers is null)
        {
            return spent;
        }

        var seller = offers.Seller;
        var needLeft = buyer.NecessityNeedPerTurn;

        foreach (var offer in offers.Offers)
        {
            if (offer.Product.Category != ProductCategory.Necessity)
            {
                continue;
            }

            while (needLeft > 0 && offer.Quantity > 0 && buyer.CanAfford(offer.Price))
            {
                if (!seller.TrySell(offer.Product, 1))
                {
                    break;
                }

                buyer.Pay(offer.Price);
                spent += offer.Price;
                needLeft--;
            }
        }

        foreach (var offer in offers.Offers)
        {
            if (offer.Product.Category != ProductCategory.Luxury)
            {
                continue;
            }

            if (needLeft > 0)
            {
                break;
            }

            if (buyer.CanAfford(offer.Price) && seller.TrySell(offer.Product, 1))
            {
                buyer.Pay(offer.Price);
                spent += offer.Price;
            }
        }

        return spent;
    }
}
