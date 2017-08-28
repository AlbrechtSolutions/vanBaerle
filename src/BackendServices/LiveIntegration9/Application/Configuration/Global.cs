using Dna.Ecommerce.LiveIntegration.Addin;
using Dna.Ecommerce.LiveIntegration.Logging;
using Dynamicweb.Core;
using Dynamicweb.Environment;

namespace Dna.Ecommerce.LiveIntegration.Configuration
{
	internal static class Global
	{
	  public static bool IsIntegrationActive
		{
			get
			{
				if (ExecutingContext.IsBackEnd())
				{
				  return false;
				}
			  return true;
			}
		}

	  public static bool IsUserSynchEnabled => Constants.UserCommunicationType.Full == Settings.Instance.UserCommunicationType;

	  public static bool IsIntegrationEnabledForShop
		{
			get
			{
				string siteShopId = "";
				var pv = Dynamicweb.Frontend.PageView.Current();
				if (!string.IsNullOrEmpty(pv?.Area?.EcomShopId))
				{
					siteShopId = pv.Area.EcomShopId;
				}
				else
				{
					Logger.Instance.Log(ErrorLevel.Error, "AreaEcomShopId not found in Area settings");
				}
				return IntegrationEnabledFor(siteShopId);
			}
		}

		public static bool IntegrationEnabledFor(string shopId)
		{
			string connectorShopId = Settings.Instance.ShopId;

			return (string.IsNullOrWhiteSpace(connectorShopId) || connectorShopId == shopId)
				&& Connector.IsWebServiceConnectionAvailable();
		}

		public static bool IsProductLazyLoad => Settings.Instance.LazyLoadProductInfo && !Converter.ToBoolean(System.Web.HttpContext.Current.Request["getproductinfo"]);

	  public static bool EnableCartCommunication(bool orderComplete)
		{
			string enableCartCommunication = Settings.Instance.CartCommunicationType;

			switch (enableCartCommunication)
			{
				case Constants.CartCommunicationType.None:
					return false;
				case Constants.CartCommunicationType.Full:
					return true;
				case Constants.CartCommunicationType.OnlyOnOrderComplete:
					return orderComplete;
				default:
					return false;
			}
		}
	}
}