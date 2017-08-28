using System;
using System.Linq;
using System.Xml;
using Dna.Ecommerce.LiveIntegration.Logging;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dna.Ecommerce.LiveIntegration.XmlRendering.Renderers;
using Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings;
using Dynamicweb.Security.UserManagement;

namespace Dna.Ecommerce.LiveIntegration.Extensions
{
  internal static class UserExtensions
  {
    internal static bool SynchronizeUsingLiveIntegration(this User user, bool saveUser = true, UserSyncMode userSyncMode = UserSyncMode.Put)
    {
      try
      {
        var userRequest = BuildUserRequest(user, new RenderUserSettings { UserSyncMode = userSyncMode, LiveIntegrationSubmitType = LiveIntegrationSubmitType.Live, ReferenceName = "UsersPut" });
        var response = Connector.UpdateUser(userRequest);
        if (response != null)
        {
          return ProcessResponse(user, saveUser, response);
        }
      }
      catch (Exception e)
      {
        Logger.Instance.Log(ErrorLevel.Error, string.Format("Error syncing user. User = {0}, Error = {1}.", user.ID, e));
      }
      return false;
    }

    private static bool ProcessResponse(User user, bool saveUser, XmlDocument response)
    {
      XmlNode itemNode = response.SelectSingleNode("//item");

      var newValue = ProcessResponse(itemNode, "AccessUserExternalId");
      if (newValue != null)
      {
        user.ExternalID = newValue;
      }

		newValue = ProcessResponse(itemNode, "AccessUserCustomerNumber");
		if (!string.IsNullOrEmpty(newValue))
		{
			if (string.Compare(user.CustomerNumber, newValue, StringComparison.InvariantCultureIgnoreCase) != 0)
			{
				Logger.Instance.Log(ErrorLevel.Error, string.Format(
					"User saved with a different CustomerNumber! User = {0}, CustomerNumber = {1}, ERP-CustomerNumber = {2}",
					user.ID, user.CustomerNumber, newValue));

				user.CustomerNumber = newValue;
			}
		}

      string email = ProcessResponse(itemNode, "AccessUserEmail");
      if (!string.IsNullOrWhiteSpace(email) && email != user.Email)
      {
        Logger.Instance.Log(ErrorLevel.Error, string.Format("User saved with a different email! User = {0}, Email = {1}, ERP-Email = {2}.", user.ID, user.Email, email));
        user.Email = email;
      }

      foreach (var cf in user.CustomFieldValues)
      {
        SetCustomField(user, itemNode, cf.CustomField.SystemName);
      }

      if (saveUser)
      {
        user.Save();
      }
      return true;
    }

    internal static string BuildUserRequest(User user, RenderUserSettings renderUserSettings)
    {
      return new UserXmlRenderer().RenderUserXml(user, renderUserSettings);
    }

    private static void SetCustomField(User user, XmlNode node, string name)
    {
      var cfvField = user.CustomFieldValues.FirstOrDefault(x => x.CustomField.SystemName == name);

      if (cfvField != null)
      {
        string newValue = ProcessResponse(node, name);
        if (newValue != null)
        {
          cfvField.Value = newValue;
        }
      }
    }

    private static string ProcessResponse(XmlNode node, string columnName)
    {
      string targetColumn = string.Format("column [@columnName='{0}']", columnName);
      var columnNode = node.SelectSingleNode(targetColumn);
      return columnNode?.InnerText;
    }
  }
}