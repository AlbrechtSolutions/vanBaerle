using System.Xml;
using Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Extensibility.Notifications;

namespace Dna.Ecommerce.LiveIntegration.Notifications
{
  public static class LiveIntegration
  {
    public static class Orders
    {
      public const string OnBeforeRenderOrderXml = "Dna.Ecommerce.LiveIntegration.Notifications.LiveIntegration.OnBeforeRenderOrderXml";
      public const string OnAfterRenderOrderXml = "Dna.Ecommerce.LiveIntegration.Notifications.LiveIntegration.OnAfterRenderOrderXml";

      public class OnBeforeRenderOrderXmlArgs : NotificationArgs
      {
        public OnBeforeRenderOrderXmlArgs(Order order, RenderOrderSettings renderSettings)
        {
          Order = order;
          RenderSettings = renderSettings;
        }
        public Order Order { get; }
        public RenderOrderSettings RenderSettings { get; }
      }
      public class OnAfterRenderOrderXmlArgs : NotificationArgs
      {
        public OnAfterRenderOrderXmlArgs(Order order, XmlDocument xmlDocument)
        {
          Order = order;
          Document = xmlDocument;
        }
        public Order Order { get; }
        public XmlDocument Document { get; }
      }
    }

    public static class OrderLines
    {
      public const string OnBeforeRenderOrderLineXml = "Dna.Ecommerce.LiveIntegration.Notifications.LiveIntegration.OnBeforeRenderOrderLineXml";
      public const string OnAfterRenderOrderLineXml = "Dna.Ecommerce.LiveIntegration.Notifications.LiveIntegration.OnAfterRenderOrderLineXml";

      public class OnBeforeRenderOrderLineXmlArgs : NotificationArgs
      {
        public OnBeforeRenderOrderLineXmlArgs(OrderLine orderLine)
        {
          OrderLine = orderLine;
        }
        public OrderLine OrderLine { get; }
      }
      public class OnAfterRenderOrderLineXmlArgs : NotificationArgs
      {
        public OnAfterRenderOrderLineXmlArgs(OrderLine orderLine, XmlNode orderLineNode)
        {
          OrderLine = orderLine;
          OrderLineNode = orderLineNode;
        }
        public OrderLine OrderLine { get; }
        public XmlNode OrderLineNode { get; }
      }
    }
  }
}
