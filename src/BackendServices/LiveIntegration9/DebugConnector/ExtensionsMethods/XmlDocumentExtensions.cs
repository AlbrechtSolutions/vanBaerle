using System.Text;
using System.Xml;

namespace DebugConnector.ExtensionsMethods
{
  /// <summary>
  /// Extensions for XL document.
  /// </summary>
  internal static class XmlDocumentExtensions
  {
    /// <summary>
    /// Creates a formatted / indented string from the XML document.
    /// </summary>
    /// <param name="doc">The document to convert.</param>
    internal static string Beautify(this XmlDocument doc)
    {
      var sw = new StringWriterWithEncoding(Encoding.UTF8);
      var settings = new XmlWriterSettings
      {
        Indent = true,
        IndentChars = "  ",
        NewLineChars = "\r\n",
        NewLineHandling = NewLineHandling.Replace,
        Encoding = Encoding.UTF8
      };
      using (var writer = XmlWriter.Create(sw, settings))
      {
        doc.Save(writer);
      }
      return sw.ToString();
    }
  }
}
