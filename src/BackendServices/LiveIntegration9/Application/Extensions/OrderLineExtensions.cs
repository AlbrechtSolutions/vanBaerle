using System;
using Dynamicweb.Ecommerce.Orders;

namespace Dna.Ecommerce.LiveIntegration.Extensions
{
	internal static class OrderLineExtensions
	{
		/// <summary>
		/// Determines if this order line represents a product (and not a discount or tax line for example). 
		/// </summary>
		public static bool IsProduct(this OrderLine orderLine)
		{
			return orderLine.Type == "0" || orderLine.Type == "2";
		}

		/// <summary>
		/// Determines if this order line represents a discount (and not a product or tax line for example).
		/// </summary>
		public static bool IsDiscount(this OrderLine orderLine)
		{
			return orderLine.Type == "1" || orderLine.Type == "3";
		}

		/// <summary>
		/// Determines if this order line represents a product discount (and not a product or tax line for example).
		/// </summary>
		public static bool IsProductDiscount(this OrderLine orderLine)
		{
			return orderLine.Type == "3";
		}

		/// <summary>
		/// Determines if this order line represents an order discount (and not a product or tax line for example).
		/// </summary>
		public static bool IsOrderDiscount(this OrderLine orderLine)
		{
			return orderLine.Type == "1";
		}

		/// <summary>
		/// Determines if this order line represents a tax line (and not a product or discount line for example).
		/// </summary>
		public static bool IsTax(this OrderLine orderLine)
		{
			return orderLine.Type == "4";
		}

		public static string GetOrderLineTypeDescription(this OrderLine orderLine)
		{
			if (orderLine.IsProduct())
			{
				return "Product";
			}
			if (orderLine.IsDiscount())
			{
				return "Discount";
			}
			if (orderLine.IsTax())
			{
				return "Tax";
			}
			throw new NotSupportedException(string.Format("Unsupported type {0}", orderLine.Type));
		}

		internal static OrderLine CloneOrderLine(this OrderLine orderLine)
		{
		  OrderLine newOrderLine = new OrderLine
		  {
		    Order = orderLine.Order,
		    Quantity = orderLine.Quantity,
		    Type = orderLine.Type,
		    ProductName = orderLine.ProductName,
		    ParentLineId = orderLine.ParentLineId,
		    UnitPrice =
		    {
		      PriceWithVAT = orderLine.UnitPrice.PriceWithVAT,
		      PriceWithoutVAT = orderLine.UnitPrice.PriceWithoutVAT
		    }
		  };

		  newOrderLine.Price.PriceWithVAT = orderLine.Price.PriceWithVAT;
			newOrderLine.Price.PriceWithoutVAT = orderLine.Price.PriceWithoutVAT;

			return newOrderLine;
		}
	}
}