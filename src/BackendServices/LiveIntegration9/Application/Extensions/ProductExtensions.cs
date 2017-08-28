using Dynamicweb.Ecommerce.Products;
using System;
using System.Reflection;

namespace Dna.Ecommerce.LiveIntegration.Extensions
{
	public static class ProductExtensions
	{
		private static PropertyInfo[] _propertyInfos;

		public static string PropertiesToString(this Product product)
		{
		  if (_propertyInfos == null)
		  {
		    _propertyInfos = product.GetType().GetProperties();
		  }

			var builder = new System.Text.StringBuilder();

			foreach (var info in _propertyInfos)
			{
				builder.Append(info.Name + ": ");
				try
				{
					var value = info.GetValue(product, null) ?? "(null)";
					builder.Append(value);
				}
				catch (Exception error)
				{
					builder.Append("Error=" + error.Message);
				}
				builder.Append("|");
			}
			return builder.ToString();
		}
	}
}