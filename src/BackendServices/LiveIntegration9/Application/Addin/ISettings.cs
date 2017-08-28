using Dna.Ecommerce.LiveIntegration.Configuration;

namespace Dna.Ecommerce.LiveIntegration.Addin
{
  /// <summary>
  /// Interface to define settings for the Live Integration.
  /// </summary>
  public interface ISettings
  {
    /// <summary>
    /// Gets or sets a value that determines if custom order fields are appended to the outgoing XML request.
    /// </summary>
    bool AddOrderFieldsToRequest { get; set; }

    /// <summary>
    /// Gets or sets a value that determines if custom order line fields are appended to the outgoing XML request.
    /// </summary>
    bool AddOrderLineFieldsToRequest { get; set; }

    /// <summary>
    /// Gets or sets a value that determines if parts information (BOM) is appended to the outgoing XML request.
    /// </summary>
    bool AddOrderLinePartsToRequest { get; set; }

    /// <summary>
    /// Gets or sets a value that determines if custom product fields are appended to the outgoing XML request.
    /// </summary>
    bool AddProductFieldsToRequest { get; set; }

    /// <summary>
    /// Gets or sets the customer name used in integration scenarios with anonymous users.
    /// </summary>
    string AnonymousUserKey { get; set; }

    /// <summary>
    /// Gets or sets the type of communication used for the cart. Valid options are defined in <see cref="Constants.CartCommunicationType"/>.
    /// </summary>
    string CartCommunicationType { get; set; }

    /// <summary>
    /// Gets or sets the ERP connection timeout in seconds.
    /// </summary>
    int ConnectionTimeout { get; set; }

    /// <summary>
    /// Gets or sets a value that determines if (live) cart communication is enabled for anonymous users.
    /// </summary>
    bool EnableCartCommunicationForAnonymousUsers { get; set; }

    /// <summary>
    /// Gets or sets a value that determines if live prices are retrieved from the ERP.
    /// </summary>
    bool EnableLivePrices { get; set; }

    /// <summary>
    /// Gets or sets a value that determines the type of user to sync. <see cref="Constants.UserSyncType"/>.
    /// </summary>
    string UserTypesToSync { get; set; }

    /// <summary>
    /// Gets or sets a value that determines when user data is synced to the ERP. <see cref="Constants.UserCommunicationType"/>.
    /// </summary>
    string UserCommunicationType { get; set; }

    bool UpdateUserAfterNewOrder { get; set; }

    bool LazyLoadProductInfo { get; set; }

    bool LiveProductInfoForAnonymousUsers { get; set; }

    bool LogConnectionErrors { get; set; }

    bool LogDebugInfo { get; set; }

    bool LogGeneralErrors { get; set; }

    int LogMaxSize { get; set; }

    bool LogResponseErrors { get; set; }

    string NotificationEmail { get; set; }

    string NotificationEmailSenderEmail { get; set; }

    string NotificationEmailSenderName { get; set; }

    string NotificationEmailSubject { get; set; }

    string NotificationSendingFrequency { get; set; }

    string NotificationTemplate { get; set; }

    string OrderCacheLevel { get; set; }

    string OrderStateAfterExportSucceeded { get; set; }

    string OrderStateAfterExportFailed { get; set; }

    string ProductCacheLevel { get; set; }

    bool QueueOrdersToExport { get; set; }

    bool SaveCopyOfOrderXml { get; set; }

    string SecurityKey { get; set; }

    string ShopId { get; set; }

    string TextForDiscounts { get; set; }


    string WebServiceURI { get; set; }
  }
}