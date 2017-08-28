using System.Globalization;
using System.Web;
using Dynamicweb.Controls.Extensibility;
using Dynamicweb.Controls;
using Dynamicweb.SystemTools;

namespace Dna.Ecommerce.LiveIntegration.Addin.RibbonBar
{
    /// <summary>
    /// Serves as the base class for all RibbonBar AddIns.
    /// </summary>
    public abstract class RibbonBarAddInBase : RibbonBarAddIn
    {
        internal const string LiveIntegrationTab = "DWNA Live Integration";

        protected static void StreamFile(string xml, string fileName)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}", fileName));
            HttpContext.Current.Response.AddHeader("Content-Length", xml.Length.ToString(CultureInfo.InvariantCulture));
            HttpContext.Current.Response.ContentType = "application/octet-stream";
            HttpContext.Current.Response.Write(xml);
            HttpContext.Current.Response.End();
        }

        protected RibbonBarAddInBase(Dynamicweb.Controls.RibbonBar ribbon) : base(ribbon) { }


        protected RibbonBarGroup CreateLiveIntegrationRibbon(string groupName)
        {
            return base.Ribbon.CreateGroup(LiveIntegrationTab, groupName);
        }


        #region export buttons

        public static RibbonBarButton CreateTransferOrderButton(RibbonBarGroup group)
        {
            var button = new RibbonBarButton
            {
                Text = Translate.Translate("Transfer to ERP"),
                Image = Dynamicweb.Controls.Icons.Icon.Type.Export,
                Size = Dynamicweb.Controls.Icons.Icon.Size.Large
            };

            group.AddItem(button);
            return button;
        }

        public static RibbonBarButton CreateUserSyncButton(RibbonBarGroup group)
        {
            var button = new RibbonBarButton
            {
                Text = Translate.Translate("Sync Shipping Location"),
                Image = Dynamicweb.Controls.Icons.Icon.Type.Export,
                Size = Dynamicweb.Controls.Icons.Icon.Size.Large
            };
            group.AddItem(button);
            return button;
        }

        #endregion

        #region generate XML

        public static RibbonBarButton CreateExportToXmlButton(RibbonBarGroup group, string buttonText)
        {
            var button = new RibbonBarButton
            {
                Text = Translate.Translate(buttonText),
                Image = Dynamicweb.Controls.Icons.Icon.Type.DocumentNotebook,
                Size = Dynamicweb.Controls.Icons.Icon.Size.Large
            };
            group.AddItem(button);
            return button;
        }

        #endregion
    }
}