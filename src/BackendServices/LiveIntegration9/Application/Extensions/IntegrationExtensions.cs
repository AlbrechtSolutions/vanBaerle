using System;
using System.Globalization;

namespace Dna.Ecommerce.LiveIntegration.Extensions
{
  internal static class IntegrationExtensions
  {
    private const string DateTimeFormatToXml = "yyyy/MM/dd HH:mm:ss";
    private const string DateFormatToXml = "yyyy/MM/dd";

    /// <summary>
    /// Rounds the value to the specified number of decimals and then formats it using <see cref="GetNumberFormatter"/>.
    /// </summary>
    /// <param name="value">The value to process.</param>
    /// <param name="numberOfDecimals">The number of decimals.</param>
    internal static string ToIntegrationString(this double value, int numberOfDecimals = 2)
    {
      return Math.Round(value, numberOfDecimals).ToString(GetNumberFormatter());
    }

    /// <summary>
    /// Formats the date time using <see cref="DateTimeFormatToXml"/> and <see cref="DateFormatToXml"/>.
    /// </summary>
    /// <param name="value">The value to process.</param>
    /// <param name="excludeTime">When true, the time component is not included.</param>
    internal static string ToIntegrationString(this DateTime value, bool excludeTime = false)
    {
      return value.ToString(excludeTime ? DateFormatToXml : DateTimeFormatToXml);
    }

    private static NumberFormatInfo GetNumberFormatter()
    {
      return NumberFormatInfo.InvariantInfo;
    }
  }
}