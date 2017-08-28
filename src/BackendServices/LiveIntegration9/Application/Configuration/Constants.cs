namespace Dna.Ecommerce.LiveIntegration.Configuration
{
  internal static class Constants
  {
    public const string AddInName = "ERPLiveIntegrationAddIn";

    public static readonly string WebServiceConnectionCacheKey = AddInName + "_WebServiceConnectionCacheKey";

    internal static class CacheConfiguration
    {
      public const string FetchProductInfoResult = "FetchProductInfoResult";
      public const string OrderCommunicationHash = "OrderCommunicationHash";
    }

    internal static class CartCommunicationType
    {
      public const string None = "None";
      public const string Full = "Full";
      public const string OnlyOnOrderComplete = "Only in Order Complete";
    }

    internal static class UserCommunicationType
    {
      public const string None = "None";
      public const string Full = "On User Save";
      public const string OrderSubmit2NewUsers = "On Order submit (for new users)";
      public const string OrderSubmitAlways = "On Order submit (force sync)";
    }

    internal static class UserSyncType
    {
      public const string None = "None";
      public const string All = "All";
      public const string LoginUsers = "Only login users";
      public const string ImpersonateUsers = "Only impersonated users";
    }
  }
}