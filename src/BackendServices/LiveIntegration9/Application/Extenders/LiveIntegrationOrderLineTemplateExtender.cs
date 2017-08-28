using Dynamicweb.Ecommerce.Frontend;
using Dynamicweb.Ecommerce.Orders;

namespace Dna.Ecommerce.LiveIntegration.Extenders
{
	public class LiveIntegrationOrderLineTemplateExtender : OrderLineTemplateExtender
	{
		public override void ExtendTemplate(Dynamicweb.Rendering.Template template)
		{
			if (RenderingState == TemplateExtenderRenderingState.Before && template.TagExists("Integration:Order.OrderLine.Integration.HasAvailability"))
			{
				template.SetTag("Integration:Order.OrderLine.Integration.HasAvailability", OrderLine.Product.Stock > 0);
			}
		}
	}
}