using System;

namespace Dna.Ecommerce.LiveIntegration
{
  public class ProductPrice
  {
    public string Id { get; set; }

    public string ProductId { get; set; }

    public string ProductVariantId { get; set; }

    public double? Quantity { get; set; }

    public double? Amount { get; set; } // this should've been decimal, but Dw is using doubles instead of decimals unfortunately

    public string UserCustomerNumber { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }
  }
}