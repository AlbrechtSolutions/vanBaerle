using System.Collections.Generic;
using Dna.Ecommerce.LiveIntegration.Configuration;
using Dynamicweb.Ecommerce.Products;
using Dynamicweb.Extensibility.Notifications;
using Dynamicweb.Security.UserManagement;
using Dna.Ecommerce.LiveIntegration.Extensions;
using System.Linq;
using System;
using Dna.Ecommerce.LiveIntegration.Addin;
using Dna.Ecommerce.LiveIntegration.XmlRendering;

namespace Dna.Ecommerce.LiveIntegration.NotificationSubscribers
{
	/// <summary>
	/// Base notification subscriber for Live Integration, for common functionality.
	/// </summary>
	public abstract class IntegrationBaseNotificationSubscriber : NotificationSubscriber
	{
		protected static bool EnabledAndActive()
		{
			return Global.IsIntegrationActive
			  && Global.IsIntegrationEnabledForShop
			  && Connector.IsWebServiceConnectionAvailable();
		}

        protected void UpdateOrder(Dynamicweb.Ecommerce.Orders.Order order, string notification)
        {
            //todo: implement try catch
            if (order != null)
            {
                if (Global.EnableCartCommunication(order.Complete))
                {
                    OrderHandler.UpdateOrder(order, LiveIntegrationSubmitType.LiveOrderOrCart);
                }
                else
                {
                    CheckOrderPrices(order, "Line.Decreased");
                }
            }
        }

        protected bool CantCheckPrice
		{
			get
			{
				return (!Settings.Instance.EnableLivePrices
					|| !Global.IsIntegrationActive
					|| !Settings.Instance.LiveProductInfoForAnonymousUsers && User.GetCurrentUser() == null
					|| !Global.IsIntegrationEnabledForShop
					|| User.GetCurrentUser() != null && User.GetCurrentUser().IsLivePricesDisabled
					|| !Connector.IsWebServiceConnectionAvailable()
					|| Global.IsProductLazyLoad);
			}
		}

		/// <summary>
		/// Sets the product information in Dynamicweb from the ERP.
		/// </summary>
		/// <param name="product">The product to be updated.</param>
		protected void SetProductInformation(Product product)
		{
			SetProductsInfo(new List<Product> { product });
		}

		/// <summary>
		/// Sets the product information in Dynamicweb for all the products passed.
		/// </summary>
		/// <param name="products">The products to be updated.</param>
		protected void SetProductInformation(List<Product> products, User user = null)
		{
			SetProductsInfo(products, user);
		}

		private static void SetProductsInfo(List<Product> products, User user = null)
		{
			if (!PrepareProductInfoProvider.FetchProductInfos(products, user))
			{
				return;
			}

			// Set values
			foreach (Product product in products)
			{
				PrepareProductInfoProvider.FillProductValues(product);
			}
		}

		/// <summary>
		/// Clears and sets the product information in Dynamicweb for all the products in the order.
		/// </summary>
		/// <param name="order"></param>
		protected void SetProductInformation(Dynamicweb.Ecommerce.Orders.Order order)
		{
			if (order == null)
			{
				return;
			}

			// clear product cache to ensure refresh from ERP
			Dynamicweb.Ecommerce.Integration.ErpResponseCache.ClearAllCaches();
			PrepareProductInfoProvider.ClearUserCache();

			// read all products in the order
			var productsToUpdate = order.OrderLines.Where(ol => ol.IsProduct()).Select(ol => ol.Product);
			if (productsToUpdate != null && productsToUpdate.Any())
			{
				Logging.Logger.Instance.Log(Logging.ErrorLevel.DebugInfo, string.Format("Reload prices. products #{0}", productsToUpdate.Count()));
				User orderUser = User.GetUserByID(order.CustomerAccessUserId);
				SetProductsInfo(productsToUpdate.ToList(), orderUser);
			}
		}

		internal static void UpdateProductInformation(Dynamicweb.Ecommerce.Orders.Order order)
		{
			if (order == null || !order.OrderLines.Any(ol => ol.IsProduct()))
			{
				// nothing to update
				return;
			}

			// clear product cache to ensure refresh from ERP
			Dynamicweb.Ecommerce.Integration.ErpResponseCache.ClearAllCaches();
			PrepareProductInfoProvider.ClearUserCache();

			// read all products in the order
			var productsToUpdate = order.OrderLines.Where(ol => ol.IsProduct()).Select(ol => ol.Product).ToList();
			if (productsToUpdate.Any())
			{
				Logging.Logger.Instance.Log(Logging.ErrorLevel.DebugInfo, string.Format("Reload prices. products #{0}", productsToUpdate.Count()));
				var orderUser = User.GetUserByID(order.CustomerAccessUserId);
				SetProductsInfo(productsToUpdate.ToList(), orderUser);
			}
		}

		protected static void CheckOrderPrices(Dynamicweb.Ecommerce.Orders.Order order, string notification)
		{
			if (order == null || order.OrderLines.Count <= 0)
				return;

			uint productsCount = 0; //updatedProducts = 0,
			Dictionary<Product, double> products = new Dictionary<Product, double>(order.OrderLines.Count);

			//LogToFile.Log(string.Format("read ({0}) order id = {1} lines# = {2}", notification, order.ID, order.OrderLines.Count)
			//	, global.LogFolder, LogToFile.LogType.ManyEntriesPerFile);

			foreach (var ol in order.OrderLines)
			{
				if (ol.IsProduct())
				{
					++productsCount;
					//LogToFile.Log(string.Format("read ({0}) product {1} line id = {2}", notification, ol.Product.ID, ol.ID)
					//	, global.LogFolder, LogToFile.LogType.ManyEntriesPerFile);

					if (!products.ContainsKey(ol.Product))
						products.Add(ol.Product, ol.Quantity);
				}
			}

			if (PrepareProductInfoProvider.FetchProductInfos(products.Keys.ToList()))
			{
				foreach (var p in products)
				{
                    var product = p.Key;

					//LogToFile.Log(string.Format("update ({0}) product {1}", notification, p.ID)
					//	, global.LogFolder, LogToFile.LogType.ManyEntriesPerFile);
					PrepareProductInfoProvider.FillProductValues(product, p.Value);
					var orderline = order.OrderLines.First(ol => ol.ProductId == product.Id);

					if (product.Price.PriceWithVAT == orderline.UnitPrice.PriceWithVAT
						&& product.Price.PriceWithoutVAT == orderline.UnitPrice.PriceWithoutVAT)
					{
						if (orderline.Price.PriceWithVAT == orderline.Quantity * orderline.UnitPrice.PriceWithVAT
							&& orderline.Price.PriceWithoutVAT == orderline.Quantity * orderline.UnitPrice.PriceWithoutVAT)
							continue;
					}

					//DEBUG
					//++updatedProducts;

					orderline.AllowOverridePrices = true;
					orderline.SetUnitPrice(product.Price, false);
					orderline.Type = Convert.ToString(Convert.ToInt32(Dynamicweb.Ecommerce.Orders.OrderLineType.Fixed));
					orderline.Price.PriceWithVAT = orderline.Quantity * orderline.UnitPrice.PriceWithVAT;
					orderline.Price.PriceWithoutVAT = orderline.Quantity * orderline.UnitPrice.PriceWithoutVAT;
				}
			}

			order.ForcePriceRecalculation();
			//LogToFile.Log(string.Format("finished ({0}) order id = {1} products# = {2} updated = {3}", notification, order.ID
			//	, productsCount, updatedProducts), global.LogFolder, LogToFile.LogType.ManyEntriesPerFile);
		}
	}
}