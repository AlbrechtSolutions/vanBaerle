using System.Linq;
using Dynamicweb.Extensibility.Notifications;

namespace Dna.Ecommerce.LiveIntegration.NotificationSubscribers
{
  [Subscribe(Dynamicweb.Ecommerce.Notifications.Ecommerce.ProductList.BeforeSort)]
  public class ProductListBeforeSort : IntegrationBaseNotificationSubscriber
  {
    public override void OnNotify(string notification, NotificationArgs args)
    {
      if (CantCheckPrice)
      {
        return;
      }

      var myArgs = (Dynamicweb.Ecommerce.Notifications.Ecommerce.ProductList.BeforeSortArgs)args;

      // Set product info
      if (myArgs.Products.Any())
      {
        SetProductInformation(myArgs.Products.ToList());
      }
    }
  }
}