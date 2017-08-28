namespace Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings
{
  public interface IRenderSettings
  {
    LiveIntegrationSubmitType LiveIntegrationSubmitType { get; set; }
    string ReferenceName { get; set; }
  }
}