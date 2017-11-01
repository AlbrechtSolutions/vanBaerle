using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dynamicweb.Ecommerce.Prices;


namespace Dna.Ecommerce.LiveIntegration
{
    public static class PriceFacade
    {
        public static string FormatPrice(double value)
        {
            var price = new PriceInfo();
            price.PriceWithoutVAT = value;
            return price.PriceWithoutVATFormatted;
        }
    }
}