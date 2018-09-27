using Dynamicweb.Extensibility.Notifications;

namespace Dna.Ecommerce.LiveIntegration.NotificationSubscribers
{
  [Subscribe(Dynamicweb.Ecommerce.Notifications.Ecommerce.Product.BeforeRender)]
  public class ProductBeforeRender : IntegrationBaseNotificationSubscriber
  {
    public override void OnNotify(string notification, NotificationArgs args)
    {
      if (CantCheckPrice)
      {
        return;
      }

      var myArgs = (Dynamicweb.Ecommerce.Notifications.Ecommerce.Product.BeforeRenderArgs)args;

      // Set product info
      if (!string.IsNullOrEmpty(myArgs.Product?.Number))
      {
            SetProductInformation(myArgs.Product);
      }
    }
  }
}