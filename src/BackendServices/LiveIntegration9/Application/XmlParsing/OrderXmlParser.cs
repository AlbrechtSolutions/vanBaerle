using System;
using System.Xml;

namespace Dna.Ecommerce.LiveIntegration.XmlParsing
{
  internal class OrderXmlParser : XmlParser
  {
    internal OrderXmlParser(XmlDocument xmlDocument) : base(xmlDocument)
    {
    }

    internal OrderNode SelectOrderNode()
    {
      var ordersNode = XmlDocument.SelectSingleNode("//item [@table='EcomOrders']");
      if (ordersNode == null)
      {
        throw new Exception("No element <item table=\"EcomOrders\"> found in response XML.");
      }
      return new OrderNode(ordersNode);
    }
  }

  internal class OrderNode
  {
    internal OrderNode(XmlNode orderNode)
    {
    }
  }

}