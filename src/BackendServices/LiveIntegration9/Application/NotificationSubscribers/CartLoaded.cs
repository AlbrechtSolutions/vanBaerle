using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dynamicweb.Extensibility.Notifications;

namespace Dna.Ecommerce.LiveIntegration.NotificationSubscribers
{
    [Subscribe(Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.Loaded)]
    public class CartLoaded : IntegrationBaseNotificationSubscriber
    {
        public override void OnNotify(string notification, NotificationArgs args)
        {
            const string NotificationName = "Cart.Loaded";
            var myArgs = args as Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.LoadedArgs;

            if (myArgs?.Cart == null
                || myArgs.Cart.OrderLines.Count <= 0
                || !EnabledAndActive())
            {
                return;
            }

            if (Global.EnableCartCommunication(myArgs.Cart.Complete))
            {
                // load cart when "Keep cart in context after checkout step" is on
                // if order is completed and exported to the ERP no need to update again
                if (myArgs.Cart.Complete && myArgs.Cart.IsExported)
                {
                    return;
                }

                OrderHandler.UpdateOrder(myArgs.Cart, LiveIntegrationSubmitType.LiveOrderOrCart);
            }
            else
            {
                CheckOrderPrices(myArgs.Cart, NotificationName);
            }
        }
    }
}