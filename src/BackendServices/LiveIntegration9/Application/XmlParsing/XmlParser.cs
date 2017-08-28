using System;
using System.Xml;

namespace Dna.Ecommerce.LiveIntegration.XmlParsing
{
  public class XmlParser
  {
    protected readonly XmlDocument XmlDocument;

    protected XmlParser(XmlDocument xmlDocument)
    {
      if (xmlDocument == null)
      {
        throw new ArgumentNullException(nameof(xmlDocument), $"{nameof(xmlDocument)} is null.");
      }
      XmlDocument = xmlDocument;
    }
  }
}