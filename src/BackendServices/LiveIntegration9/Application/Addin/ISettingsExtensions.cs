using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dynamicweb.Extensibility.AddIns;

namespace Dna.Ecommerce.LiveIntegration.Addin
{
  internal static class ISettingsExtensions
  {
    internal static string ToXml(this ISettings settings)
    {
      var document = new XDocument(new XDeclaration("1.0", "utf-8", string.Empty));
      var root = new XElement("Parameters");
      document.Add(root);

      var properties = typeof(LiveIntegrationAddIn).GetProperties(BindingFlags.Public | BindingFlags.Instance);

      foreach (var p in properties.Where(prop => Attribute.IsDefined(prop, typeof(AddInParameterAttribute))))
      {
        var value = p.GetValue(settings);
        var name = p.GetCustomAttribute<AddInParameterAttribute>().Name;
        root.Add(CreateParameterNode(typeof(LiveIntegrationAddIn), name, value?.ToString() ?? ""));
      }
      return document.ToString();
    }

    private static XElement CreateParameterNode(System.Type parameterType, string name, string value)
    {
      if ((parameterType != null) && !string.IsNullOrEmpty(name))
      {
        if (!string.IsNullOrEmpty(value))
        {
          return new XElement("Parameter", new XAttribute("addin", parameterType.FullName), new XAttribute("name", name), new XAttribute("value", value));
        }
        return new XElement("Parameter", new XAttribute("addin", parameterType.FullName), new XAttribute("name", name), new XAttribute("value", ""));
      }
      return null;
    }
  }
}