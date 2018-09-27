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

      var products = myArgs.Products.Where(p => !string.IsNullOrEmpty(p.Number)).ToList();
      
      // Set product info
      if (products.Any())
      {
        SetProductInformation(products);
      }
    }
  }
}