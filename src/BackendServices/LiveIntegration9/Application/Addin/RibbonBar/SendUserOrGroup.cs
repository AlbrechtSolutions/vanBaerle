using System;
using System.Web;
using Dna.Ecommerce.LiveIntegration.Extensions;
using Dynamicweb.Controls.Extensibility;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Security.UserManagement;

namespace Dna.Ecommerce.LiveIntegration.Addin.RibbonBar
{
  [AddInName("Sync user")]
  [AddInDescription("Synchronize user with ERP")]
  [AddInTarget(RibbonBarAddInTarget.UserManagement.UserEdit)]
  public class SendUserOrGroup : RibbonBarAddInBase
	{
		public SendUserOrGroup(Dynamicweb.Controls.RibbonBar ribbon) : base(ribbon) { }

		public override void Load()
		{
		  var user = Ribbon.DataContext.DataSource as User;
		  if (user == null)
		  {
		    return;
		  }
			var button = CreateUserSyncButton(base.CreateLiveIntegrationRibbon("Users Sync"));
			button.OnClientClick = "if(!confirm('Sync user with ERP?')) { return false; };";
			button.EnableServerClick = true;
			button.Click += SyncObject;
		}

		public override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			if (HttpContext.Current.Session[SessionKey] != null && !string.IsNullOrEmpty(HttpContext.Current.Session[SessionKey].ToString()))
			{
				writer.Write("<script>alert('{0}')</script>", HttpUtility.HtmlEncode(HttpContext.Current.Session[SessionKey]));
				HttpContext.Current.Session.Remove(SessionKey);
			}
			base.Render(writer);
		}

		private string SessionKey
		{
			get
			{
				string key = "LiveIntegration.TransferredUserOrGroup";
				if (Ribbon.DataContext.DataSource is User)
				{
					var source = (User)Ribbon.DataContext.DataSource;
					key += "User" + source.ID;
				}
				else if (Ribbon.DataContext.DataSource is Group)
				{
					var source = (Group)Ribbon.DataContext.DataSource;
					key += "Group" + source.ID;
				}

				return key;
			}
		}

		private void SyncObject(object sender, EventArgs e)
		{
		  var user = Ribbon.DataContext.DataSource as User;
		  if (user == null)
		  {
		    return;
		  }
      var result = user.SynchronizeUsingLiveIntegration();
      HttpContext.Current.Session[SessionKey] = string.Format("The user has {0} been synced with the ERP!", result ? "successfully" : "unsuccessfully");
		}
	}
}