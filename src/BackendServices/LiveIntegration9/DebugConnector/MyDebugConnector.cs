using System;
using System.IO;
using System.Xml;
using DebugConnector.ExtensionsMethods;
using DynamicwebServiceClass;
using NLog;

namespace DebugConnector
{
  /// <summary>
  /// Custom connector to exchange data with the Boxx ERP.
  /// </summary>
  public class MyDebugConnector : Connector
  {
    /// <summary>
    /// NLog logger to log data.
    /// </summary>
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private const string LogFolder = "Logs";

    /// <summary>
    /// Processes the request by getting data from production or the test environment.
    /// </summary>
    /// <param name="request">The XML message that needs to be processed.</param>
    public override string ProcessRequest(string request)
    {
      try
      {
        _logger.Debug("Executing request: {0}.", request);
        var requestParser = new RequestParser(request);
        if (requestParser.ReturnProducts)
        {
          return "Products"; 
        }
        if (requestParser.SaveOrders)
        {
          var xml = new XmlDocument();
          xml.LoadXml(request);
          DumpFile(xml.Beautify());
          return "Saving orders";
        }
        return "Unknown request: " + request;
      }
      catch (Exception ex)
      {
        _logger.Error(ex, string.Format("Error processing request {0}.", request));
        return string.Format("An error occurred while processing your request: {0}.", ex.Message);
      }
    }

    private static void DumpFile(string request)
    {
      string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFolder);
      File.WriteAllText(Path.Combine(folder, Guid.NewGuid() + ".txt"), request);
    }
  }
}
