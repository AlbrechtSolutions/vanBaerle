namespace Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings
{
  public class RenderOrderSettings: IRenderSettings
  {
    public bool AddOrderLineFieldsToRequest { get; set; }
    public bool AddOrderFieldsToRequest { get; set; }
    public bool CreateOrder { get; set; }
    public bool Beautify { get; set; }
    public LiveIntegrationSubmitType LiveIntegrationSubmitType { get; set; }
    public string ReferenceName { get; set; }
  }
}