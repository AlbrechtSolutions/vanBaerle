using System.Linq;
using Dynamicweb.Extensibility.Notifications;

namespace Dna.Ecommerce.LiveIntegration.NotificationSubscribers
{
  [Subscribe(Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.OrderIsPassedToCheckoutHandler)]
  public class LiveIntegrationCartOrderIsPassedToCheckoutHandlerObserver : IntegrationBaseNotificationSubscriber
  {
    public override void OnNotify(string notification, NotificationArgs args)
    {
      var orderPassedArgs = (Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.OrderIsPassedToCheckoutHandlerArgs)args;

      if (orderPassedArgs == null)
      {
        return;
      }

      //undone check if order makes payment before order completed (and check queue flag)
    }
  }
}