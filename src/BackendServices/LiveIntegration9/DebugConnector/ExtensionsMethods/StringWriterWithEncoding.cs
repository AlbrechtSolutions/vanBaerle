using System.IO;
using System.Text;

namespace DebugConnector.ExtensionsMethods
{
  //http://stackoverflow.com/questions/427725/how-to-put-an-encoding-attribute-to-xml-other-that-utf-16-with-xmlwriter
  public sealed class StringWriterWithEncoding : StringWriter
  {
    public StringWriterWithEncoding(Encoding encoding)
    {
      Encoding = encoding;
    }

    public override Encoding Encoding { get; }
  }
}
