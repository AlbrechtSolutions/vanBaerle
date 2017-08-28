using System.Web;
using Dna.Ecommerce.LiveIntegration.XmlRendering;

namespace Dna.Ecommerce.LiveIntegration
{
	public static class TemplatesHelper
	{
		public static bool IsWebServiceConnectionAvailable()
		{
			return Connector.IsWebServiceConnectionAvailable();
		}

		public static bool HasErrorOnExportOrder()
		{
			bool bValue;

			if (HttpContext.Current != null && HttpContext.Current.Session != null
				&& bool.TryParse(HttpContext.Current.Session["LiveIntegration.OrderExportFailed"].ToString(), out bValue))
			{
			  return bValue;
			}

		  return false;
		}

		public static string FailedOrderId()
		{
			return HttpContext.Current.Session["LiveIntegration.FailedOrderId"].ToString();
		}

    /// <summary>
    /// Method to update order in ERP.
    /// </summary>
    /// <param name="orderId">id to read order</param>
    /// <returns>null if no communication has made, or bool if order has updated or not</returns>
    public static bool? UpdateOrder(string orderId)
		{
			return UpdateOrder(Dynamicweb.Ecommerce.Orders.Order.GetOrderById(orderId));
		}

		/// <summary>
		/// Method to update order in ERP.
		/// </summary>
		/// <param name="order">order to update in the ERP</param>
		/// <returns>null if no communication has made, or bool if order has updated or not</returns>
		public static bool? UpdateOrder(Dynamicweb.Ecommerce.Orders.Order order)
		{
			return OrderHandler.UpdateOrder(order, LiveIntegrationSubmitType.FromTemplates);
		}


		/// <summary>
		/// Pick an existing order and updates product information on it
		/// </summary>
		/// <param name="orderId">id to read order</param>
		public static void UpdateStockOnOrder(string orderId)
		{
			UpdateStockOnOrder(Dynamicweb.Ecommerce.Orders.Order.GetOrderById(orderId));
		}

		/// <summary>
		/// Updates product information on the order
		/// </summary>
		/// <param name="order">order to be have stock validated</param>
		public static void UpdateStockOnOrder(Dynamicweb.Ecommerce.Orders.Order order)
		{
			// call LI method to check stocks in ERP
			NotificationSubscribers.IntegrationBaseNotificationSubscriber.UpdateProductInformation(order);
		}

	}
}