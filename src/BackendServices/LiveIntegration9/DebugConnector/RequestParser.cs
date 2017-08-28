using System.Xml;

namespace DebugConnector
{
  /// <summary>
  /// Helper class to parse request coming from Dynamicweb's task scheduler for Batch integration.
  /// </summary>
  public class RequestParser
  {
    // Sample of data that we receive:
    //  <GetEcomData TargetEnvironment="Production"><tables><Products type="all"/><Currencies type="all"/><Languages type="all"/><Manufacturers  type="all"/><Units type="all"/></tables></GetEcomData>
    private readonly XmlDocument _document = new XmlDocument();

    /// <summary>
    /// Creates a new instance of the RequestParser class.
    /// </summary>
    /// <param name="request">The XML request to parse.</param>
    public RequestParser(string request)
    {      
      _document.LoadXml(request);
    }

    /// <summary>
    /// Gets a value that determines if products should be retrieved and returned.
    /// </summary>
    public bool ReturnProducts => NodeExists("GetEcomData/tables/Products");

    /// <summary>
    /// Gets a value that determines if products should be retrieved and returned.
    /// </summary>
    public bool SaveOrders => NodeExists("/tables[@source='LiveIntegration'");

    /// <summary>
    /// Gets a value that determines which products should be returned. This is typically "all".
    /// </summary>
    public string ProductType => GetValueForTypeAttribute("GetEcomData/tables/Products");

    private bool NodeExists(string nodeName)
    {
      return GetNode(nodeName) != null;
    }

    private string GetValueForTypeAttribute(string nodeName)
    {
      var node = GetNode(nodeName);
      if (node?.Attributes == null)
      {
        return "";
      }
      var typeAttribute = node.Attributes["type"];
      return typeAttribute == null ? "" : typeAttribute.Value;
    }

    private XmlNode GetNode(string nodeName)
    {
      return _document.SelectSingleNode(nodeName);
    }
  }
}