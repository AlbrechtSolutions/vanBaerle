using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dynamicweb.Extensibility.Notifications;

namespace Dna.Ecommerce.LiveIntegration.NotificationSubscribers
{
  [Subscribe(Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.Line.Increased)]
  public class OrderLineIncreased : IntegrationBaseNotificationSubscriber
	{
		public override void OnNotify(string notification, NotificationArgs args)
		{
			if (args == null
				|| !(args is Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.Line.IncreasedArgs)
				|| !EnabledAndActive())
			{
				return;
			}

			var myArgs = (Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.Line.IncreasedArgs)args;

			if (!base.CantCheckPrice)
			{
				SetProductInformation(myArgs.IncreasedLine.Product);
			}

			if (myArgs.Cart != null)
			{
				if (Global.EnableCartCommunication(myArgs.Cart.Complete))
				{
					OrderHandler.UpdateOrder(myArgs.Cart, LiveIntegrationSubmitType.LiveOrderOrCart);
				}
				else
				{
					CheckOrderPrices(myArgs.Cart, "Line.Increased");
				}
			}
		}
	}
}