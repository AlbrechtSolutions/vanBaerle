using System.Xml;
using Dna.Ecommerce.LiveIntegration.Extensions;
using Dna.Ecommerce.LiveIntegration.ExtensionsMethods;
using Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings;
using Dynamicweb.Security.UserManagement;
using Dynamicweb.Security.UserManagement.Common.SystemFields;

namespace Dna.Ecommerce.LiveIntegration.XmlRendering.Renderers
{
  /// <summary>
  /// Renderer class to generate an XML document for a user.
  /// </summary>
  internal class UserXmlRenderer : XmlRenderer
  {
    /// <summary>
    /// Generates an XML document for the given user.
    /// </summary>
    /// <param name="user">The user instance for which the XML should be generated.</param>
    /// <param name="settings">A settings object to drive the rendering.</param>
    internal string RenderUserXml(User user, RenderUserSettings settings)
    {
      var xmlDocument = BuildXmlDocument();
      var tablesNode = CreateAndAppendTablesNode(xmlDocument, settings);
      tablesNode.AppendChild(BuildUserXml(xmlDocument, user, settings));
      tablesNode.AppendChild(BuildSystemFields(xmlDocument, user));
      tablesNode.AppendChild(BuildUserAddressesXml(xmlDocument, user));
      if (settings.Beautify)
      {
        return xmlDocument.Beautify();
      }
      return xmlDocument.InnerXml;
    }

    private XmlNode BuildUserAddressesXml(XmlDocument xmlDocument, User user)
    {
      var tableNode = CreateTableNode(xmlDocument, "AccessUserAddress");
      foreach (var address in user.Addresses)
      {
        CreateAddressXml(tableNode, address);
      }
      return tableNode;
    }

    private static XmlNode BuildUserXml(XmlDocument xmlDocument, User user, RenderUserSettings settings)
    {
      var tableNode = CreateTableNode(xmlDocument, "AccessUser");
      var itemNode = CreateAndAppendItemNode(tableNode, "AccessUser");

      itemNode.SetAttribute("userMode", settings.UserSyncMode.ToString());

      AddChildXmlNode(itemNode, "AccessUserId", user.ID.ToString());
      AddChildXmlNode(itemNode, "AccessUserCustomerNumber", user.CustomerNumber);
      AddChildXmlNode(itemNode, "AccessUserExternalId", user.ExternalID);
      AddChildXmlNode(itemNode, "AccessUserUserName", user.UserName);
      AddChildXmlNode(itemNode, "AccessUserName", user.Name);
      AddChildXmlNode(itemNode, "AccessUserTitle", user.Title);
      AddChildXmlNode(itemNode, "AccessUserFirstName", user.FirstName);
      AddChildXmlNode(itemNode, "AccessUserMiddleName", user.MiddleName);
      AddChildXmlNode(itemNode, "AccessUserLastName", user.LastName);
      AddChildXmlNode(itemNode, "AccessUserDepartment", user.Department);
      AddChildXmlNode(itemNode, "AccessUserEmail", user.Email);
      AddChildXmlNode(itemNode, "AccessUserPhone", user.Phone);
      AddChildXmlNode(itemNode, "AccessUserFax", user.Fax);
      AddChildXmlNode(itemNode, "AccessUserType", user.UserType.ToString());
      AddChildXmlNode(itemNode, "AccessUserValidFrom", user.ValidFrom.ToIntegrationString());
      AddChildXmlNode(itemNode, "AccessUserValidTo", user.ValidTo.ToIntegrationString());
      AddChildXmlNode(itemNode, "AccessUserAddress", user.Address);
      AddChildXmlNode(itemNode, "AccessUserAddress2", user.Address2);
      AddChildXmlNode(itemNode, "AccessUserHouseNumber", user.HouseNumber);
      AddChildXmlNode(itemNode, "AccessUserZip", user.Zip);
      AddChildXmlNode(itemNode, "AccessUserCity", user.City);
      AddChildXmlNode(itemNode, "AccessUserState", user.State);
      AddChildXmlNode(itemNode, "AccessUserCountry", user.Country);
      AddChildXmlNode(itemNode, "AccessUserJobTitle", user.JobTitle);
      AddChildXmlNode(itemNode, "AccessUserCompany", user.Company);
      AddChildXmlNode(itemNode, "AccessUserPhonePrivate", user.PhonePrivate);
      AddChildXmlNode(itemNode, "AccessUserMobile", user.PhoneMobile);
      AddChildXmlNode(itemNode, "AccessUserCurrencyCode", user.Currency);
      AddChildXmlNode(itemNode, "AccessUserActive", user.Active.ToString());
      AddChildXmlNode(itemNode, "AccessUserImage", user.Image);
      AddChildXmlNode(itemNode, "AccessUserBusiness", user.PhoneBusiness);
      AddChildXmlNode(itemNode, "AccessUserShopId", user.ShopID);
      AddChildXmlNode(itemNode, "AccessUserPointBalance", user.PointBalance.ToIntegrationString());
      AddChildXmlNode(itemNode, "AccessUserGeoLocationLat", user.GeolocationLatitude.ToIntegrationString(6));
      AddChildXmlNode(itemNode, "AccessUserGeoLocationLng", user.GeolocationLongitude.ToIntegrationString(6));
      AddChildXmlNode(itemNode, "AccessUserGeoLocationIsCustom", user.IsGeolocationCustom.ToString());
      AddChildXmlNode(itemNode, "AccessUserGeoLocationHash", user.GeolocationHashCode);
      AddChildXmlNode(itemNode, "AccessUserGeoLocationImage", user.GeolocationIcon);
      AddChildXmlNode(itemNode, "AccessUserNewsletterAllowed", user.EmailAllowed.ToString());
      AddChildXmlNode(itemNode, "AccessUserCreatedOn", user.CreatedOn?.ToIntegrationString() ?? string.Empty);
      AddChildXmlNode(itemNode, "AccessUserUpdatedOn", user.UpdatedOn?.ToIntegrationString() ?? string.Empty);
      AddChildXmlNode(itemNode, "AccessUserCreatedBy", user.CreatedByUser);
      AddChildXmlNode(itemNode, "AccessUserUpdatedBy", user.UpdatedByUser);
      AddChildXmlNode(itemNode, "AccessUserEmailPermissionGivenOn", user.EmailPermissionGivenOn?.ToIntegrationString() ?? string.Empty);
      AddChildXmlNode(itemNode, "AccessUserEmailPermissionUpdatedOn", user.EmailPermissionUpdatedOn?.ToIntegrationString() ?? string.Empty);
      AddChildXmlNode(itemNode, "AccessUserVatRegNumber", user.VatRegNumber);
      AddChildXmlNode(itemNode, "AccessUserDisableLivePrices", user.IsLivePricesDisabled.ToString());
      AddChildXmlNode(itemNode, "AccessUserLastLoginOn", user.LastLogOnOn?.ToIntegrationString() ?? string.Empty);
      AddChildXmlNode(itemNode, "AccessUserExported", user.Exported?.ToIntegrationString() ?? string.Empty);

      RenderCustomFields(itemNode, user);

      tableNode.AppendChild(itemNode);
      return tableNode;
    }

    private static void RenderCustomFields(XmlElement itemNode, User user)
    {
      foreach (var customField in user.CustomFieldValues)
      {
        AddChildXmlNode(itemNode, customField.CustomField.SystemName, customField.Value.ToString());
      }
    }

    private XmlNode BuildSystemFields(XmlDocument xmlDocument, User user)
    {
      var tableNode = CreateTableNode(xmlDocument, "SystemFieldValue");
      foreach (var field in user.SystemFieldValues)
      {
        CreateSystemFieldXml(tableNode, field);
      }
      return tableNode;
    }

    private static void CreateSystemFieldXml(XmlNode tableNode, SystemFieldValue fieldValue)
    {
      var itemNode = CreateAndAppendItemNode(tableNode, "SystemFieldValue");

      AddChildXmlNode(itemNode, "SystemFieldValueSystemName", fieldValue.SystemField.SystemName);
      AddChildXmlNode(itemNode, "SystemFieldValueValue", fieldValue.Value.ToString());
      AddChildXmlNode(itemNode, "SystemFieldValueItemId", fieldValue.ItemId.ToString());
    }

    private static void CreateAddressXml(XmlNode tableNode, UserAddress address)
    {
      var itemNode = CreateAndAppendItemNode(tableNode, "AccessUserAddress");

      AddChildXmlNode(itemNode, "AccessUserAddressId", address.ID.ToString());
      AddChildXmlNode(itemNode, "AccessUserAddressIsDefault", address.IsDefault.ToString());
      AddChildXmlNode(itemNode, "AccessUserAddressUserId", address.UserID.ToString());
      AddChildXmlNode(itemNode, "AccessUserAddressCustomerNumber", address.CustomerNumber);
      AddChildXmlNode(itemNode, "AccessUserAddressUniqueId", address.UniqueIdentifier);
      AddChildXmlNode(itemNode, "AccessUserAddressType", address.AddressType.ToString());
      AddChildXmlNode(itemNode, "AccessUserAddressCallName", address.CallName);
      AddChildXmlNode(itemNode, "AccessUserAddressName", address.Name);
      AddChildXmlNode(itemNode, "AccessUserAddressCompany", address.Company);
      AddChildXmlNode(itemNode, "AccessUserAddressAddress", address.Address);
      AddChildXmlNode(itemNode, "AccessUserAddressAddress2", address.Address2);
      AddChildXmlNode(itemNode, "AccessUserAddressZip", address.Zip);
      AddChildXmlNode(itemNode, "AccessUserAddressCity", address.City);
      AddChildXmlNode(itemNode, "AccessUserAddressState", address.State);
      AddChildXmlNode(itemNode, "AccessUserAddressCountry", address.Country);
      AddChildXmlNode(itemNode, "AccessUserAddressCountryCode", address.CountryCode);
      AddChildXmlNode(itemNode, "AccessUserAddressEmail", address.Email);
      AddChildXmlNode(itemNode, "AccessUserAddressPhone", address.Phone);
      AddChildXmlNode(itemNode, "AccessUserAddressPhoneBusiness", address.PhoneBusiness);
      AddChildXmlNode(itemNode, "AccessUserAddressCell", address.Cell);
      AddChildXmlNode(itemNode, "AccessUserAddressFax", address.Fax);

      RenderCustomAddressFields(itemNode, address);
    }

    private static void RenderCustomAddressFields(XmlElement itemNode, UserAddress address)
    {
      foreach (var customField in address.CustomFieldValues)
      {
        AddChildXmlNode(itemNode, customField.CustomField.SystemName, customField.Value.ToString());
      }
    }
  }
}