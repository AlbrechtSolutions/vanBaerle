using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dynamicweb.Extensibility.Notifications;

namespace Dna.Ecommerce.LiveIntegration.NotificationSubscribers
{
  [Subscribe(Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.CheckoutDoneOrderIsComplete)]
  public class OrderComplete : IntegrationBaseNotificationSubscriber
	{
		public override void OnNotify(string notification, NotificationArgs args)
		{
			if (args == null
				|| !(args is Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.CheckoutDoneOrderIsCompleteArgs)
				|| !Global.IsIntegrationActive
				|| !Connector.IsWebServiceConnectionAvailable())
			{
			  return;
			}

		  var myArgs = (Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.CheckoutDoneOrderIsCompleteArgs)args;


			// context not defined with (Authorize.net) checkout handler
			if (Global.IntegrationEnabledFor(myArgs.Order.ShopId)
				&& Global.EnableCartCommunication(myArgs.Order.Complete))
			{
				var result = OrderHandler.UpdateOrder(myArgs.Order, LiveIntegrationSubmitType.LiveOrderOrCart);

				// clear cached prices to update stock if order is completed with success
				if (result.HasValue && result.Value)
				{
					base.SetProductInformation(myArgs.Order);
				}
			}
		}
	}
}