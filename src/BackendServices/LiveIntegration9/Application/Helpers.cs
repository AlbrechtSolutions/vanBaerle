using System;
using System.Xml;
using Dna.Ecommerce.LiveIntegration.Logging;
using Dynamicweb.Ecommerce.Products;

namespace Dna.Ecommerce.LiveIntegration
{
	internal static class Helpers
	{
		public static string ProductIdentifier(Product product)
		{
			return ProductIdentifier(product.Id, product.VariantId, product.LanguageId, product.Number);
		}

		public static string ProductIdentifier(string productId, string variantId, string languageId, string productNumber)
		{
		  if (!string.IsNullOrEmpty(productNumber)) // TODO Review: This is taken from CRS. Make configurable?
		  {
		    return productNumber;
		  }
			return $"{productId}.{variantId}.{languageId}";
		}

		public static void AddChildXmlNodeToProductList(XmlDocument xDoc, XmlElement parent, string nodeName, string nodeValue)
		{
			XmlElement node = xDoc.CreateElement(nodeName);
			node.InnerText = nodeValue;
			parent.AppendChild(node);
		}

		/// <summary>
		/// Returns the relevant Enum Value. If input can be parsed to Enum type, then the enum
		/// value is returned. Otherwise defaultValue is returned.
		/// </summary>
		/// <typeparam name="T">Type of Enum to parse value from</typeparam>
		/// <param name="input">To be parsed as Enum value.</param>
		/// <param name="defaultValue">Default value if input was not valid enum value.</param>
		/// <returns></returns>
		public static T GetEnumValueFromString<T>(string input, T defaultValue)
		{
			if (string.IsNullOrEmpty(input))
				return defaultValue;

			T value;
			try
			{
				value = (T)Enum.Parse(typeof(T), input, true);
			}
			catch (Exception)
			{
				return defaultValue;
			}

			return value;
		}

		public static bool ParseResponseToXml(string response, out XmlDocument responseDoc)
		{
			bool result = true;
			responseDoc = new XmlDocument();
			try
			{
				responseDoc.LoadXml(response);
			}
			catch (Exception e)
			{
				result = false;
				Logger.Instance.Log(ErrorLevel.ResponseError, string.Format("Response is not valid XML format. Error: '{0}' xmlstring={1}.", e.Message, response));
			}
			return result;
		}

		internal static double ReadDouble(string value)
		{
			double temp;
		  if (double.TryParse(value, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.InvariantCulture, out temp))
		  {
		    return temp;
		  }

		  if (double.TryParse(value, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.GetCultureInfo("pt-PT"), out temp))
		  {
		    return temp;
		  }
			return 0;
		}
	}
}