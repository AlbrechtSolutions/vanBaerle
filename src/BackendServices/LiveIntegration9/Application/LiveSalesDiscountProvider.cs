using System.Web;
using Dna.Ecommerce.LiveIntegration.Extensions;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Ecommerce.Orders.SalesDiscounts;
using Dynamicweb.Extensibility.AddIns;

namespace Dna.Ecommerce.LiveIntegration
{
  [AddInName("Live integration discount")]
  [AddInDescription("Apply discounts retrieved via live integration")]
  public class LiveSalesDiscountProvider : SalesDiscountProvider
  {
    public override bool HideDiscountValueGroupBox
    {
      get { return true; }
      set { base.HideDiscountValueGroupBox = true; }
    }

    public override void ProcessOrder(Order order)
    {
      if (HttpContext.Current.Session["LiveIntegrationDiscounts" + order.Id] != null)
      {
        if (HttpContext.Current.Session["LiveIntegrationDiscounts" + order.Id] != null)
        {
          var discounts = (OrderLineCollection)HttpContext.Current.Session["LiveIntegrationDiscounts" + order.Id];

          foreach (var ol in discounts)
          {
            order.OrderLines.Add(ol.CloneOrderLine(), false);
          }
        }
      }
    }
  }
}