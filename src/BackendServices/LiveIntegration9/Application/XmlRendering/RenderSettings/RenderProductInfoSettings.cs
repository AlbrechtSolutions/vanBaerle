namespace Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings
{
  public class RenderProductInfoSettings : IRenderSettings
  {
    public bool Beautify { get; set; }
    public LiveIntegrationSubmitType LiveIntegrationSubmitType { get; set; }
    public string ReferenceName { get; set; }
    public bool AddProductFieldsToRequest { get; set; }
  }
}