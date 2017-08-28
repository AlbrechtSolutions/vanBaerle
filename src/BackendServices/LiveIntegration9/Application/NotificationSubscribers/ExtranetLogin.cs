using Dna.Ecommerce.LiveIntegration.Addin;
using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.Extensions;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dynamicweb.Extensibility.Notifications;
using Dynamicweb.Security.UserManagement;

namespace Dna.Ecommerce.LiveIntegration.NotificationSubscribers
{
  [Subscribe(Dynamicweb.Notifications.Standard.User.OnExtranetLogin)]
  public class ExtranetLogin : IntegrationBaseNotificationSubscriber
	{
		public override void OnNotify(string notification, NotificationArgs args)
		{
			if (Global.IsIntegrationActive && Connector.IsWebServiceConnectionAvailable())
			{
				//Used to load the cart and apply the discount after the user logged-in and has not empty cart
				var myArgs = (Dynamicweb.Notifications.Standard.User.OnExtranetLoginArgs)args;
				var order = Dynamicweb.Ecommerce.Common.Context.Cart;

				if (myArgs.User != null)
				{
					UrlHandler.Instance.ClearCachedUrl();
					Dynamicweb.Ecommerce.Integration.ErpResponseCache.ClearAllCaches();
					PrepareProductInfoProvider.ClearUserCache();

					//check if user has updates
					CheckUser(myArgs.User);
				}

				if (order != null && order.IsCart && EnabledAndActive())
				{
					if (Global.EnableCartCommunication(order.Complete))
					{
						OrderHandler.UpdateOrder(order, LiveIntegrationSubmitType.LiveOrderOrCart);
					}
					else
					{
						CheckOrderPrices(order, "OnExtranetLogin");
					}
				}
			}
		}

		private void CheckUser(User user)
		{
			string user2Sync = Settings.Instance.UserTypesToSync;

			// confirm if user should be sync
			if (user == null
			  // sync users on Login not active
			  || user2Sync == Constants.UserSyncType.None
			  // is impersonating and only login users are sync
			  || user.CurrentSecondaryUser != null && user2Sync == Constants.UserSyncType.LoginUsers
			  // is not impersonating and only impersonate users are sync
			  || user.CurrentSecondaryUser == null && user2Sync == Constants.UserSyncType.ImpersonateUsers)
				return;

			// communicate to ERP to update user (new request)
			user.SynchronizeUsingLiveIntegration(true, UserSyncMode.Get);
		}
	}
}