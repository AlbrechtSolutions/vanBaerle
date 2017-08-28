using System;
using System.Xml.Serialization;
using Dna.Ecommerce.LiveIntegration.Configuration;

namespace Dna.Ecommerce.LiveIntegration.Addin
{
  public class Settings : ISettings
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Settings"/> class.
    /// </summary>
    public Settings()
    {
      // default values
      LazyLoadProductInfo = true;
      AddProductFieldsToRequest = true;

      QueueOrdersToExport = true;
      AddOrderFieldsToRequest = true;
      AddOrderLineFieldsToRequest = true;

      UserCommunicationType = Constants.UserCommunicationType.None;
      UserTypesToSync = Constants.UserSyncType.All;
      LiveProductInfoForAnonymousUsers = true;
      EnableCartCommunicationForAnonymousUsers = true;
      AnonymousUserKey = "Anonymous";
      UpdateUserAfterNewOrder = false;
      TextForDiscounts = "Live integration discount";
    }

    public static Settings Instance => ConfigurationHandler.GetSettings();

    public static DateTime LastNotificationEmailSent { get; set; }

    public static void UpdateFrom(ISettings source, ISettings target)
    {
      target.WebServiceURI = source.WebServiceURI;
      target.ShopId = source.ShopId;
      target.SecurityKey = source.SecurityKey;
      target.ConnectionTimeout = source.ConnectionTimeout;
      target.TextForDiscounts = source.TextForDiscounts;

      target.LiveProductInfoForAnonymousUsers = source.LiveProductInfoForAnonymousUsers;
      target.EnableCartCommunicationForAnonymousUsers = source.EnableCartCommunicationForAnonymousUsers;
      target.AnonymousUserKey = source.AnonymousUserKey;
      target.UpdateUserAfterNewOrder = source.UpdateUserAfterNewOrder;
      target.UserCommunicationType = source.UserCommunicationType;
      target.UserTypesToSync = source.UserTypesToSync;
      target.EnableLivePrices = source.EnableLivePrices;

      target.ProductCacheLevel = source.ProductCacheLevel;
      target.OrderCacheLevel = source.OrderCacheLevel;
      target.OrderStateAfterExportSucceeded = source.OrderStateAfterExportSucceeded;
      target.OrderStateAfterExportFailed = source.OrderStateAfterExportFailed;

      target.SaveCopyOfOrderXml = source.SaveCopyOfOrderXml;
      target.LogConnectionErrors = source.LogConnectionErrors;
      target.LogResponseErrors = source.LogResponseErrors;
      target.LogGeneralErrors = source.LogGeneralErrors;
      target.LogDebugInfo = source.LogDebugInfo;
      target.LogMaxSize = source.LogMaxSize;

      target.NotificationEmail = source.NotificationEmail;
      target.NotificationTemplate = source.NotificationTemplate;
      target.NotificationEmailSubject = source.NotificationEmailSubject;
      target.NotificationEmailSenderName = source.NotificationEmailSenderName;
      target.NotificationEmailSenderEmail = source.NotificationEmailSenderEmail;
      target.NotificationSendingFrequency = source.NotificationSendingFrequency;

      target.AddOrderFieldsToRequest = source.AddOrderFieldsToRequest;
      target.AddOrderLineFieldsToRequest = source.AddOrderLineFieldsToRequest;
      target.AddProductFieldsToRequest = source.AddProductFieldsToRequest;

      target.LazyLoadProductInfo = source.LazyLoadProductInfo;
      target.QueueOrdersToExport = source.QueueOrdersToExport;
      target.AddOrderLinePartsToRequest = source.AddOrderLinePartsToRequest;
      target.CartCommunicationType = source.CartCommunicationType;

      if (target.LogMaxSize == 0)
      {
        target.LogMaxSize = 10; // MB
      }
    }
    #region Configuration parameters
    #region General parameters
    public string WebServiceURI { get; set; }

    public string SecurityKey { get; set; }

    public int ConnectionTimeout { get; set; }
    public string ShopId { get; set; }

    #endregion

    #region Products parameters

    public bool EnableLivePrices { get; set; }

    public bool LazyLoadProductInfo { get; set; }

    public bool AddProductFieldsToRequest { get; set; }

    public string ProductCacheLevel { get; set; }
    #endregion

    #region Orders parameters

    public string CartCommunicationType { get; set; }

    public bool QueueOrdersToExport { get; set; }

    public bool AddOrderFieldsToRequest { get; set; }

    public bool AddOrderLineFieldsToRequest { get; set; }


    public bool AddOrderLinePartsToRequest { get; set; }

    public string TextForDiscounts { get; set; }


    public string OrderStateAfterExportSucceeded { get; set; }

    public string OrderStateAfterExportFailed { get; set; }


    public string OrderCacheLevel { get; set; }

    #endregion

    #region Users parameters


    public string UserCommunicationType { get; set; }


    public string UserTypesToSync { get; set; }

    public bool LiveProductInfoForAnonymousUsers { get; set; }


    public bool EnableCartCommunicationForAnonymousUsers { get; set; }
    public string AnonymousUserKey { get; set; }


    public bool UpdateUserAfterNewOrder { get; set; }

    #endregion

    #region Notifications parameters


    public string NotificationEmail { get; set; }


    public string NotificationTemplate { get; set; }


    public string NotificationEmailSubject { get; set; }


    public string NotificationEmailSenderName { get; set; }


    public string NotificationEmailSenderEmail { get; set; }

    public string NotificationSendingFrequency { get; set; }

    #endregion

    #region Logs parameters


    public bool SaveCopyOfOrderXml { get; set; }


    public int LogMaxSize { get; set; }


    public bool LogGeneralErrors { get; set; }


    public bool LogConnectionErrors { get; set; }


    public bool LogResponseErrors { get; set; }


    public bool LogDebugInfo { get; set; }
    #endregion
    #endregion
  }
}