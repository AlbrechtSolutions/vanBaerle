using System;
using Dna.Ecommerce.LiveIntegration.Extensions;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings;
using Dynamicweb.Controls.Extensibility;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Security.UserManagement;

namespace Dna.Ecommerce.LiveIntegration.Addin.RibbonBar
{
  [AddInTarget(RibbonBarAddInTarget.UserManagement.UserEdit)]
  public class CreateXmlForUserOrGroupAddIn : RibbonBarAddInBase
  {
    /// <summary>
    /// Adds an Export XML button to the Ribbon bar for users in the backend.
    /// </summary>
    public CreateXmlForUserOrGroupAddIn(Dynamicweb.Controls.RibbonBar ribbon) : base(ribbon) { }

    public override void Load()
    {
      var user = Ribbon.DataContext.DataSource as User;
      if (user == null)
      {
        return;
      }
      var button = CreateExportToXmlButton(base.CreateLiveIntegrationRibbon("Users Sync"), "Export user");
      button.EnableServerClick = true;
      button.Click += CreateXml;
    }

    private void CreateXml(object sender, EventArgs e)
    {
      var user = Ribbon.DataContext.DataSource as User;
      if (user == null)
      {
        return;
      }
      var source = user;
      var xml = UserExtensions.BuildUserRequest(User.GetUserBySql("select * from AccessUser where accessuserid = " + source.ID), new RenderUserSettings { UserSyncMode = UserSyncMode.Put, Beautify = true, LiveIntegrationSubmitType = LiveIntegrationSubmitType.DownloadedFromBackend, ReferenceName = "UsersPut"});
      StreamFile(xml, string.Format("User_{0}.xml", source.ID));
    }
  }
}