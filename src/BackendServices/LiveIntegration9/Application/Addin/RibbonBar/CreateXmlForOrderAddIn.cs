using System;
using System.IO;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dna.Ecommerce.LiveIntegration.XmlRendering.Renderers;
using Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings;
using Dynamicweb.Controls.Extensibility;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Extensibility.AddIns;

namespace Dna.Ecommerce.LiveIntegration.Addin.RibbonBar
{
    /// <summary>
    /// Adds Export XML buttons to the Ribbon bar for order details.
    /// </summary>
    [AddInName("Create XML")]
    [AddInDescription("Creates XML for an order")]
    [AddInTarget(RibbonBarAddInTarget.eCom.OrderEdit)]
    public class CreateXmlForOrderAddIn : RibbonBarAddInBase
    {
        private static bool SaveOrderXml => Settings.Instance.SaveCopyOfOrderXml;

        /// <summary>
        /// Creates a new instance of the CreateXmlForOrderAddIn class.
        /// </summary>
        /// <param name="ribbon">The ribbon to which the controls are added.</param>
        public CreateXmlForOrderAddIn(Dynamicweb.Controls.RibbonBar ribbon) : base(ribbon) { }

        /// <summary>
        /// Fires when this ribbon bar add in loads.
        /// </summary>
        public override void Load()
        {
            var order = Ribbon.DataContext.DataSource as Order;
            if (order == null)
            {
                return;
            }
            var group = base.CreateLiveIntegrationRibbon("Orders");

            var buttonOriginalXml = CreateExportToXmlButton(group, "Original XML");
            var enableButton = SaveOrderXml && File.Exists(BuildXmlFileName(order));
            buttonOriginalXml.Disabled = !enableButton;
            if (enableButton)
            {
                buttonOriginalXml.Title = "Downloads the original XML for an order as sent to the ERP";
                buttonOriginalXml.EnableServerClick = true;
                buttonOriginalXml.Click += CreateOriginalXml;
            }
            else
            {
                if (!SaveOrderXml)
                {
                    buttonOriginalXml.Title = "This option is not available because saving XML files is not enabled in the Live Integration setup.";
                }
                else
                {
                    buttonOriginalXml.Title = "This option is not available because the XML file does not exist.";
                }
                buttonOriginalXml.EnableServerClick = false;
            }

            var buttonCurrentXml = CreateExportToXmlButton(group, "Current XML");
            buttonCurrentXml.Title = "Exports the order to an XML document";
            buttonCurrentXml.EnableServerClick = true;
            buttonCurrentXml.Click += CreateCurrentXml;
        }

        private static string BuildXmlFileName(Order order)
        {
            return OrderHandler.BuildXmlCopyPath(order.Id, OrderHandler.GetLogFolderForXmlCopies(order.CompletedDate));
        }

        private void CreateOriginalXml(object sender, EventArgs e)
        {
            var order = (Order)Ribbon.DataContext.DataSource;
            var xml = File.ReadAllText(BuildXmlFileName(order));
            StreamFile(xml, string.Format("Order_{0}.xml", order.Id));
        }

        /// <summary>
        /// Occurs when the button was clicked from edit order page. Gets the XML for the order and returns it to the browser.
        /// </summary>        
        private void CreateCurrentXml(object sender, EventArgs e)
        {
            var order = (Order)Ribbon.DataContext.DataSource;
            var xml = new OrderXmlRenderer().RenderOrderXml(order, new RenderOrderSettings { AddOrderFieldsToRequest = true, AddOrderLineFieldsToRequest = true, CreateOrder = true, Beautify = true, LiveIntegrationSubmitType = LiveIntegrationSubmitType.DownloadedFromBackend, ReferenceName = "OrdersPut" });
            StreamFile(xml, string.Format("Order_{0}.xml", order.Id));
        }
    }
}