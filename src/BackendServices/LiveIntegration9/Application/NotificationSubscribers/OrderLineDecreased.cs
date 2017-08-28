using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dynamicweb.Extensibility.Notifications;

namespace Dna.Ecommerce.LiveIntegration.NotificationSubscribers
{
  [Subscribe(Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.Line.Decreased)]
  public class OrderLineDecreased : IntegrationBaseNotificationSubscriber
	{
		public override void OnNotify(string notification, NotificationArgs args)
		{
			if (args == null
				|| !(args is Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.Line.DecreasedArgs)
				|| !EnabledAndActive())
			{
				return;
			}

			var myArgs = (Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.Line.DecreasedArgs)args;

            base.UpdateOrder(myArgs?.Cart, "Line.Decreased");

			//if (myArgs.Cart != null)
			//{
			//	if (Global.EnableCartCommunication(myArgs.Cart.Complete))
			//	{
			//		OrderHandler.UpdateOrder(myArgs.Cart, LiveIntegrationSubmitType.LiveOrderOrCart);
			//	}
			//	else
			//	{
			//		CheckOrderPrices(myArgs.Cart, "Line.Decreased");
			//	}
			//}
		}
	}
}