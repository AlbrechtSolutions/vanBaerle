using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Dna.Ecommerce.LiveIntegration.Logging;
using Dynamicweb.Rendering;
using Dynamicweb.Security.UserManagement;

namespace Dna.Ecommerce.LiveIntegration
{
  internal class IntegrationCustomerCenterHandler
  {
    private static readonly string OrderLinesLoop = "OrderLinesLoop";

    public static Template RetrieveItemDetailsFromRemoteSystem(Template template, string callType, User user, string itemId)
    {
      if (!string.IsNullOrEmpty(callType) && user != null)
      {
        string request = GetItemDetailsRequest(callType, user.ExternalID, itemId);
        XmlDocument response = Connector.RetrieveDataFromRequestString(request);
        if (response != null && !string.IsNullOrEmpty(response.InnerXml))
        {
          if (template.TemplateBaseType != Template.TemplateType.Xslt)
          {
            ProcessItemDetailsResponse(response, template, callType);
          }
          else
          {
            MakeXslTransformation(response, template);
          }
        }
      }
      return template;
    }

    public static Template RetrieveItemsListFromRemoteSystem(Template template, string callType, User user, int pageSize, int pageIndex, out int totalItemsCount)
    {
      totalItemsCount = 0;
      if (!string.IsNullOrEmpty(callType) && user != null)
      {
        string request = GetRequest(callType, true, user.ExternalID, null, pageSize, pageIndex);
        XmlDocument response = Connector.RetrieveDataFromRequestString(request);
        if (response != null && !string.IsNullOrEmpty(response.InnerXml))
        {
          if (template.TemplateBaseType != Template.TemplateType.Xslt)
          {
            ProcessItemsListResponse(response, template, callType, pageSize, pageIndex, out totalItemsCount);
          }
          else
          {
            MakeXslTransformation(response, template);
          }
        }
      }
      return template;
    }

    private static string GetItemDetailsRequest(string callType, string userId, string itemId)
    {
      return GetRequest(callType, false, userId, itemId, 0, 0);
    }

    private static string GetRequest(string callType, bool isList, string userId, string itemId, int pageSize, int pageIndex)
    {
      string ret = string.Format("type=\"{0}\" customerID=\"{1}\"", callType, userId);
      if (isList)
      {
        int firstItem = pageSize * pageIndex - pageSize + 1;
        if (firstItem <= 0)
          firstItem = 1;

        ret = string.Format("<GetList {0} requestAmount=\"{1}\" firstItem=\"{2}\"></GetList>", ret, pageSize, firstItem);
      }
      else if (!string.IsNullOrEmpty(itemId))
      {
        ret = string.Format("<GetItem {0}  documentNO=\"{1}\"></GetItem>", ret, itemId);
      }
      return ret;
    }

    private static void ProcessItemDetailsResponse(XmlDocument response, Template template, string callType)
    {
      string tagName = string.Empty;

      try
      {
        XmlNode orderNode = response.SelectSingleNode("//item [@table='EcomOrders']");
        //Set Order tags
        if (orderNode != null && orderNode.ChildNodes.Count > 0)
        {
          foreach (XmlNode itemProperty in orderNode.ChildNodes)
          {
            if (itemProperty.Attributes["columnName"] != null)
            {
              tagName = itemProperty.Attributes["columnName"].Value;
              template.SetTag(tagName, itemProperty.InnerText);
            }
          }
        }
      }
      catch (Exception e)
      {
        Logger.Instance.Log(ErrorLevel.Error, string.Format("Error setting order tags: error='{0}' tag='{1}.'"
          , e.Message, tagName));
      }

      try
      {
        //Set OrderLines loop
        XmlNodeList orderLinesNodes = response.SelectNodes("//item [@table='EcomOrderLines']");
        if (orderLinesNodes != null && orderLinesNodes.Count > 0)//Process OrderLines
        {
          Template orderLinesLoop = template.GetLoop(OrderLinesLoop);

          foreach (XmlNode orderLineNode in orderLinesNodes)
          {
            foreach (XmlNode itemProperty in orderLineNode.ChildNodes)
            {
              if (itemProperty.Attributes["columnName"] != null)
              {
                tagName = itemProperty.Attributes["columnName"].Value;
                orderLinesLoop.SetTag(tagName, itemProperty.InnerText);
              }
            }
            orderLinesLoop.CommitLoop();
          }
        }
      }
      catch (Exception e)
      {
        Logger.Instance.Log(ErrorLevel.Error, string.Format("Error setting order line tags: error='{0}' tag='{1}.'"
          , e.Message, tagName));
      }
    }

    private static void ProcessItemsListResponse(XmlDocument response, Template template, string callType, int pageSize, int pageIndex, out int totalItemsCount)
    {
      totalItemsCount = 0;
      string tagPrefix = string.Format("Ecom:IntegrationCustomerCenter.{0}", callType);
      try
      {
        XmlNodeList items = response.SelectNodes("//item");

        if (items != null && items.Count > 0)
        {
          XmlNode itemsNode = response.SelectSingleNode("Items");
          if (itemsNode != null && itemsNode.Attributes["totalCount"] != null)
          {
            Int32.TryParse(itemsNode.Attributes["totalCount"].Value, out totalItemsCount);
          }
          if (totalItemsCount <= 0)
          {
            totalItemsCount = items.Count;
          }

          string itemsLoopName = string.Format("{0}Loop", tagPrefix);
          Template itemsLoop = template.GetLoop(itemsLoopName);

          foreach (XmlNode item in items)
          {
            foreach (XmlNode itemProperty in item.ChildNodes)
            {
              if (itemProperty.Attributes["columnName"] != null)
              {
                string tagName = itemProperty.Attributes["columnName"].Value;
                itemsLoop.SetTag(tagName, itemProperty.InnerText);
              }
            }
            itemsLoop.CommitLoop();
          }
        }
        else
        {
          //set empty list tag
          template.SetTag(string.Format("{0}.EmptyList", tagPrefix), true);
        }
      }
      catch (Exception e)
      {
        Logger.Instance.Log(ErrorLevel.Error, string.Format("Error processing items List: error='{0}'.", e.Message));
      }
    }

    private static void MakeXslTransformation(XmlDocument doc, Template template)
    {
      try
      {
        XslCompiledTransform xslTransform = new XslCompiledTransform();
        StringWriter writer = new StringWriter();
        xslTransform.Load(new XPathDocument(new StringReader(template.Html)));
        xslTransform.Transform(doc.CreateNavigator(), null, writer);
        template.Html = writer.ToString();
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error in xsl transformation: '{0}'.", ex.Message);
        template.Html = msg;
        Logger.Instance.Log(ErrorLevel.Error, msg);
      }
    }
  }
}