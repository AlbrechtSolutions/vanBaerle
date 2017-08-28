using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dna.Ecommerce.LiveIntegration.Addin;
using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.ExtensionsMethods;
using Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings;
using Dynamicweb.Ecommerce.Products;
using Dynamicweb.Security.UserManagement;

namespace Dna.Ecommerce.LiveIntegration.XmlRendering.Renderers
{
  internal class ProductInfoXmlRenderer : XmlRenderer
  {
    internal string RenderProductInfoXml(List<Product> products, RenderProductInfoSettings settings)
    {
      // NotificationManager.Notify(Notifications.LiveIntegration.Orders.OnBeforeRenderOrderXml, new Notifications.LiveIntegration.Orders.OnBeforeRenderOrderXmlArgs(order, settings));
      var xmlDocument = BuildXmlDocument();
      var tablesNode = CreateAndAppendTablesNode(xmlDocument, settings);
      tablesNode.AppendChild(BuildProductInfoXml(xmlDocument, tablesNode, products, settings));
      // NotificationManager.Notify(Notifications.LiveIntegration.Orders.OnAfterRenderOrderXml, new Notifications.LiveIntegration.Orders.OnAfterRenderOrderXmlArgs(order, xmlDocument));
      if (settings.Beautify)
      {
        return xmlDocument.Beautify();
      }
      return xmlDocument.InnerXml;
    }

    private XmlNode BuildProductInfoXml(XmlDocument xmlDocument, XmlElement tablesNode, List<Product> products, RenderProductInfoSettings settings)
    {
      if (products == null || !products.Any())
      {
        return null;
      }
      var currencyCode = Dynamicweb.Ecommerce.Common.Context.Currency.Code;

      var user = User.GetCurrentUser();
      tablesNode.SetAttribute("ExternalUserId", !string.IsNullOrWhiteSpace(user?.ExternalID) ? user.ExternalID : Settings.Instance.AnonymousUserKey);
      tablesNode.SetAttribute("AccessUserCustomerNumber", !string.IsNullOrWhiteSpace(user?.CustomerNumber) ? user.CustomerNumber : Settings.Instance.AnonymousUserKey);
      tablesNode.SetAttribute("type", "filter");
      var tableNode = CreateTableNode(xmlDocument, "Products");
      foreach (var product in products)
      {
        var itemNode = CreateAndAppendItemNode(tableNode, "Products");
        if (product != null)
        {
          AddChildXmlNode(itemNode, "ProductId", product.Id);
          AddChildXmlNode(itemNode, "ProductVariantId", product.VariantId);
          AddChildXmlNode(itemNode, "ProductNumber", product.Number);
          AddChildXmlNode(itemNode, "CurrencyCode", currencyCode);

          if (settings.AddProductFieldsToRequest && product.ProductFieldValues.Count > 0)
          {
            AppendProductFields(product, itemNode);
          }
        }
      }
      return tableNode;
    }

    private void AppendProductFields(Product product, XmlElement productNode)
    {
      foreach (var field in product.ProductFieldValues)
      {
        AddChildXmlNode(productNode, field.ProductField.SystemName, field.Value?.ToString() ?? string.Empty, isCustomField: true);
      }
    }
  }
}