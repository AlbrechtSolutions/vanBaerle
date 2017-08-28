using System;
using System.Collections.Generic;
using System.Xml;
using Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings;

namespace Dna.Ecommerce.LiveIntegration.XmlRendering.Renderers
{
  internal abstract class XmlRenderer
  {
    protected static XmlElement CreateAndAppendItemNode(XmlNode parent, string tableName)
    {
      if (parent.OwnerDocument == null)
      {
        throw new InvalidOperationException("Can't call this method without an XmlDocument associated with the parent element.");
      }
      var itemNode = parent.OwnerDocument.CreateElement("item");
      itemNode.SetAttribute("table", tableName);
      parent.AppendChild(itemNode);
      return itemNode;
    }

    protected static void AddChildXmlNode(XmlElement parent, string nodeName, string nodeValue, bool isInformationalOnly = false, bool isCustomField = false)
    {
      if (parent.OwnerDocument == null)
      {
        throw new InvalidOperationException("Can't call this method without an XmlDocument associated with the parent element.");
      }
      var node = parent.OwnerDocument.CreateElement("column");
      node.SetAttribute("columnName", nodeName);
      Dictionary<string, string> attributes = BuildAttributes(isInformationalOnly, isCustomField);
      foreach (var attribute in attributes)
      {
        node.SetAttribute(attribute.Key, attribute.Value);
      }
      node.InnerText = nodeValue;
      parent.AppendChild(node);
    }

    private static Dictionary<string, string> BuildAttributes(bool isInformationalOnly, bool isCustomField)
    {
      var attributes = new Dictionary<string, string>();
      if (isInformationalOnly)
      {
        attributes.Add("isInformationalOnly", true.ToString());
      }
      if (isCustomField)
      {
        attributes.Add("isCustomField", true.ToString());
      }
      return attributes;
    }

    protected static XmlElement CreateTableNode(XmlDocument xmlDocument, string tableName)
    {
      var ordersNode = xmlDocument.CreateElement("table");
      ordersNode.SetAttribute("tableName", tableName);
      return ordersNode;
    }

    protected static XmlElement CreateAndAppendTablesNode(XmlDocument xmlDocument, IRenderSettings settings)
    {
      var xmlRoot = xmlDocument.CreateElement("tables");
      xmlRoot.SetAttribute("source", "LiveIntegration");
      xmlRoot.SetAttribute("submitType", settings.LiveIntegrationSubmitType.ToString());
      xmlRoot.SetAttribute("referenceName", settings.ReferenceName);
      xmlDocument.AppendChild(xmlRoot);
      return xmlRoot;
    }

    protected static XmlDocument BuildXmlDocument()
    {
      var xDoc = new XmlDocument();
      var xDec = xDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
      xDoc.AppendChild(xDec);
      return xDoc;
    }
  }
}