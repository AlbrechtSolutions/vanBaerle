namespace Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings
{
  internal class RenderUserSettings : IRenderSettings
  {
    public bool Beautify { get; set; }
    public LiveIntegrationSubmitType LiveIntegrationSubmitType { get; set; }
    public string ReferenceName { get; set; }
    public UserSyncMode UserSyncMode { get; set; }
  }
}