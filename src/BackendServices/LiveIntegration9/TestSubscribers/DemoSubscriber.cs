using System.IO;
using Dna.Ecommerce.LiveIntegration.Notifications;
using Dynamicweb.Extensibility.Notifications;

namespace TestSubscribers
{
  [Subscribe(LiveIntegration.Orders.OnBeforeRenderOrderXml)]
  [Subscribe(LiveIntegration.Orders.OnAfterRenderOrderXml)]
  [Subscribe(LiveIntegration.OrderLines.OnBeforeRenderOrderLineXml)]
  [Subscribe(LiveIntegration.OrderLines.OnAfterRenderOrderLineXml)]
  public class DemoSubscriber : NotificationSubscriber
  {
    public override void OnNotify(string notification, NotificationArgs args)
    {
      Log("Subscriber: " + notification);
      switch (notification.ToLower())
      {
        case "dna.ecommerce.liveintegration.notifications.liveintegration.onbeforerenderorderxml":
          var localArgs = (LiveIntegration.Orders.OnBeforeRenderOrderXmlArgs)args;
          Log("  Order ID: " + localArgs.Order.Id);
          Log("  Render settings: " + localArgs.RenderSettings.Beautify);
          break;
        case "dna.ecommerce.liveintegration.notifications.liveintegration.onafterrenderorderxml":
          var localAfterArgs = (LiveIntegration.Orders.OnAfterRenderOrderXmlArgs)args;
          Log("  Order ID: " + localAfterArgs.Order.Id);
          Log("  Number of nodes:" + localAfterArgs.Document.ChildNodes.Count);
          break;
        case "dna.ecommerce.liveintegration.notifications.liveintegration.onbeforerenderorderlinexml":
          var localBeforeLineArgs = (LiveIntegration.OrderLines.OnBeforeRenderOrderLineXmlArgs)args;
          Log("  Order Line ID: " + localBeforeLineArgs.OrderLine.Id);
          break;
        case "dna.ecommerce.liveintegration.notifications.liveintegration.onafterrenderorderlinexml":
          var localAfterLineArgs = (LiveIntegration.OrderLines.OnAfterRenderOrderLineXmlArgs)args;
          Log("  Order Line ID: " + localAfterLineArgs.OrderLine.Id);
          Log("  Number of nodes:" + localAfterLineArgs.OrderLineNode.ChildNodes.Count);
          break;
      }
      base.OnNotify(notification, args);
    }

    private void Log(string log)
    {
      File.AppendAllText(@"D:\\FreeForAll\\Test.txt", log + "\r\n");
    }
  }
}
