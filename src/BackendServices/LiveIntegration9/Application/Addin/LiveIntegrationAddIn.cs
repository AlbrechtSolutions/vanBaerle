using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.Logging;
using Dynamicweb.Core;
using Dynamicweb.DataIntegration.Integration.ERPIntegration;
using Dynamicweb.Ecommerce.Integration;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Ecommerce.Shops;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Extensibility.Editors;
using Dynamicweb.Rendering;
using Dynamicweb.Security.UserManagement;

namespace Dna.Ecommerce.LiveIntegration.Addin
{
  [AddInName(Constants.AddInName)]
  [AddInLabel("ERP Live Integration")]
  [AddInIgnore(false)]
  [AddInUseParameterGrouping(true)]
  [AddInUseParameterOrdering(true)]
  public class LiveIntegrationAddIn : BaseLiveIntegrationAddIn, IDropDownOptions, ISettings
  {
    #region Configuration parameters

    #region General parameters

    [AddInParameter("Web service URL")]
    [AddInParameterEditor(typeof(TextParameterEditor), "inputClass=NewUIinput;TextArea=True;style=height:60px;")]
    [AddInParameterGroup("A) General")]
    [AddInParameterOrder(10)]
    public override string WebServiceURI { get; set; }

    [AddInParameter("Security key")]
    [AddInParameterEditor(typeof(TextParameterEditor), "inputClass=NewUIinput;")]
    [AddInParameterGroup("A) General")]
    [AddInParameterOrder(20)]
    public override string SecurityKey { get; set; }

    [AddInParameter("Connection timeout (seconds)")]
    [AddInParameterEditor(typeof(IntegerNumberParameterEditor), "NewGUI=true;none=false")]
    [AddInParameterGroup("A) General")]
    [AddInParameterOrder(30)]
    public int ConnectionTimeout { get; set; } = 30;

    [AddInParameter("Shop")]
    [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false")]
    [AddInParameterGroup("A) General")]
    [AddInParameterOrder(40)]
    public string ShopId { get; set; }

    #endregion

    #region Products parameters

    [AddInParameter("Enable live prices")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
    [AddInParameterGroup("B) Products")]
    [AddInParameterOrder(50)]
    public bool EnableLivePrices { get; set; } = true;

    [AddInParameter("Lazy load product info (&getproductinfo=true)")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
    [AddInParameterGroup("B) Products")]
    [AddInParameterOrder(60)]
    public bool LazyLoadProductInfo { get; set; }

    [AddInParameter("Include product custom fields in request")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;Value=true;")]
    [AddInParameterGroup("B) Products")]
    [AddInParameterOrder(70)]
    public bool AddProductFieldsToRequest { get; set; }

    [AddInParameter("Product information cache level")]
    [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false;Value=0")]
    [AddInParameterGroup("B) Products")]
    [AddInParameterOrder(80)]
    public string ProductCacheLevel { get; set; }
    #endregion

    #region Orders parameters

    [AddInParameter("Cart communication type")]
    [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false")]
    [AddInParameterGroup("C) Orders")]
    [AddInParameterOrder(90)]
    public string CartCommunicationType { get; set; }

    [AddInParameter("Queue orders (and allow payments) if no connection")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;Value=true;")]
    [AddInParameterGroup("C) Orders")]
    [AddInParameterOrder(100)]
    public bool QueueOrdersToExport { get; set; }

    [AddInParameter("Include order custom fields in request")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;Value=true;")]
    [AddInParameterGroup("C) Orders")]
    [AddInParameterOrder(110)]
    public bool AddOrderFieldsToRequest { get; set; }

    [AddInParameter("Include order line custom fields in request")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;Value=true;")]
    [AddInParameterGroup("C) Orders")]
    [AddInParameterOrder(120)]
    public bool AddOrderLineFieldsToRequest { get; set; }

    [AddInParameter("Include parts order lines in request")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;Value=true;")]
    [AddInParameterGroup("C) Orders")]
    [AddInParameterOrder(130)]
    public bool AddOrderLinePartsToRequest { get; set; }

    [AddInParameter("Text for discount order lines")]
    [AddInParameterEditor(typeof(TextParameterEditor), "NewGUI=true;inputClass=NewUIinput;Horizontal=true;")]
    [AddInParameterGroup("C) Orders")]
    [AddInParameterOrder(140)]
    public string TextForDiscounts { get; set; }

    [AddInParameter("Order state after export succeeded")]
    [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false;SortBy=Key;")]
    [AddInParameterGroup("C) Orders")]
    [AddInParameterOrder(150)]
    public string OrderStateAfterExportSucceeded { get; set; }

    [AddInParameter("Order state after export failed")]
    [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false;SortBy=Key;")]
    [AddInParameterGroup("C) Orders")]
    [AddInParameterOrder(160)]
    public string OrderStateAfterExportFailed { get; set; }

    [AddInParameter("Order cache level")]
    [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false;")]
    [AddInParameterGroup("C) Orders")]
    [AddInParameterOrder(170)]
    public string OrderCacheLevel { get; set; }

    #endregion

    #region Users parameters

    [AddInParameter("Synchronize on event")]
    [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false")]
    [AddInParameterGroup("D) Users")]
    [AddInParameterOrder(180)]
    public string UserCommunicationType { get; set; }

    [AddInParameter("Type of user to sync")]
    [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false")]
    [AddInParameterGroup("D) Users")]
    [AddInParameterOrder(190)]
    public string UserTypesToSync { get; set; }

    [AddInParameter("Live product info for anonymous users")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
    [AddInParameterGroup("D) Users")]
    [AddInParameterOrder(200)]
    public bool LiveProductInfoForAnonymousUsers { get; set; }

    [AddInParameter("Cart and orders for anonymous users")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
    [AddInParameterGroup("D) Users")]
    [AddInParameterOrder(210)]
    public bool EnableCartCommunicationForAnonymousUsers { get; set; }

    [AddInParameter("ERP Anonymous user key")] 
    [AddInParameterEditor(typeof(TextParameterEditor), "inputClass=NewUIinput;")]
    [AddInParameterGroup("D) Users")]
    [AddInParameterOrder(230)]
    public string AnonymousUserKey { get; set; }

    [AddInParameter("Update user on each order")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
    [AddInParameterGroup("D) Users")]
    [AddInParameterOrder(220)]
    public bool UpdateUserAfterNewOrder { get; set; }

    #endregion

    #region Notifications parameters

    [AddInParameter("Notification recipient e-mail")]
    [AddInParameterEditor(typeof(TextParameterEditor), "inputClass=NewUIinput;")]
    [AddInParameterGroup("E) Notifications")]
    [AddInParameterOrder(230)]
    public string NotificationEmail { get; set; }

    [AddInParameter("Notification e-mail template")]
    [AddInParameterEditor(typeof(TemplateParameterEditor), "folder=Templates/DataIntegration/Notifications;FullPath=true;inputClass=NewUIinput;")]
    [AddInParameterGroup("E) Notifications")]
    [AddInParameterOrder(240)]
    public string NotificationTemplate { get; set; }

    [AddInParameter("Notification e-mail subject")]
    [AddInParameterEditor(typeof(TextParameterEditor), "inputClass=NewUIinput;")]
    [AddInParameterGroup("E) Notifications")]
    [AddInParameterOrder(250)]
    public string NotificationEmailSubject { get; set; }

    [AddInParameter("Notification e-mail sender name")]
    [AddInParameterEditor(typeof(TextParameterEditor), "inputClass=NewUIinput;")]
    [AddInParameterGroup("E) Notifications")]
    [AddInParameterOrder(260)]
    public string NotificationEmailSenderName { get; set; }

    [AddInParameter("Notification sender e-mail")]
    [AddInParameterEditor(typeof(TextParameterEditor), "inputClass=NewUIinput;")]
    [AddInParameterGroup("E) Notifications")]
    [AddInParameterOrder(270)]
    public string NotificationEmailSenderEmail { get; set; }

    [AddInParameter("Notification sending frequency")]
    [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false;SortBy=Key;")]
    [AddInParameterGroup("E) Notifications")]
    [AddInParameterOrder(280)]
    public string NotificationSendingFrequency { get; set; }

    #endregion

    #region Logs parameters

    [AddInParameter("Save copy of order XML request")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
    [AddInParameterGroup("F) Logs")]
    [AddInParameterOrder(290)]
    public bool SaveCopyOfOrderXml { get; set; }

    [AddInParameter("Log file max size (MB)")]
    [AddInParameterEditor(typeof(TextParameterEditor), "inputClass=NewUIinput;")]
    [AddInParameterGroup("F) Logs")]
    [AddInParameterOrder(300)]
    public int LogMaxSize { get; set; }

    [AddInParameter("Log general errors")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
    [AddInParameterGroup("F) Logs")]
    [AddInParameterOrder(310)]
    public bool LogGeneralErrors { get; set; }

    [AddInParameter("Log connection errors")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
    [AddInParameterGroup("F) Logs")]
    [AddInParameterOrder(320)]
    public bool LogConnectionErrors { get; set; }

    [AddInParameter("Log response errors")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
    [AddInParameterGroup("F) Logs")]
    [AddInParameterOrder(330)]
    public bool LogResponseErrors { get; set; }

    [AddInParameter("Log request and response content")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
    [AddInParameterGroup("F) Logs")]
    [AddInParameterOrder(340)]
    public bool LogDebugInfo { get; set; }

    #endregion

    #endregion

    public LiveIntegrationAddIn()
    {
      ConfigurationHandler.EnsureConfigurationFolderAndFileExist();
    }

    public override string LoadSettings()
    {
      var settings = Settings.Instance;
      Settings.UpdateFrom(settings, this);
      return this.ToXml();
    }

    public override void SaveSettings()
    {
      Settings.UpdateFrom(this, Settings.Instance);
      ConfigurationHandler.SaveSettings();
	  Connector.ClearCache();
    }

    public override bool IsWebServiceConnectionAvailable()
    {
      return Connector.IsWebServiceConnectionAvailable();
    }

    public override void RetrievePDF(HttpRequest request, HttpResponse response)
    {
      string id = request["id"];
      string type = request["type"];
      bool forceDownload = Converter.ToBoolean(request["forceDownload"]);

      string requestString = string.Format("<GetPDFForItem type=\"{0}\" id=\"{1}\"", type, id);
      var user = User.GetCurrentExtranetUser();
      if (user?.ExternalID != null) requestString += $" externalUserID=\"{user.ExternalID}\"";
      requestString += "></GetPDFForItem>";
      string base64EncodedPdf = ErpServiceCaller.GetDataFromRequestString(UrlHandler.Instance.GetWebServiceUrl(), SecurityKey, requestString);

      var inputStream = new MemoryStream();
      var writer = new StreamWriter(inputStream);
      writer.Write(base64EncodedPdf);
      writer.Flush();
      inputStream.Position = 0;
      response.ContentType = "application/pdf";

      if (forceDownload)
      {
        string fileName = string.Format("IntegrationCustomerCenterItem{0}.pdf", id);
        string filePath = HttpContext.Current.Server.MapPath(string.Format("/{0}/System/Log/LiveIntegration/{1}", Dynamicweb.Content.Files.FilesAndFolders.GetFilesFolderName(), fileName));
        using (Stream stream = File.OpenWrite(filePath))
        {
          DecodeStream(inputStream, stream);
        }
        response.Clear();
        response.AddHeader("content-disposition", string.Format("attachment;filename={0}", fileName));
        response.WriteFile(filePath);
        response.Flush();
        File.Delete(filePath);
      }
      else
      {
        DecodeStream(inputStream, response.OutputStream);
        response.OutputStream.Flush();
        response.OutputStream.Close();
      }
    }

    public override List<string> GetSupportedCustomerCenterIntegrationCalls()
    {
      return new List<string> { "OpenOrder", "Invoice", "Credit" };
    }

    public override Template RetrieveItemDetailsFromRemoteSystem(Template template, string callType, User user, string itemId)
    {
      return IntegrationCustomerCenterHandler.RetrieveItemDetailsFromRemoteSystem(template, callType, user, itemId);
    }

    public override Template RetrieveItemsListFromRemoteSystem(Template template, string callType, User user, int pageSize, int pageIndex, ref int totalItemsCount)
    {
      return IntegrationCustomerCenterHandler.RetrieveItemsListFromRemoteSystem(template, callType, user, pageSize, pageIndex, out totalItemsCount);
    }

    private static void DecodeStream(Stream inStream, Stream output)
    {
      System.Security.Cryptography.ICryptoTransform transform = new System.Security.Cryptography.FromBase64Transform();
      using (var cryptStream = new System.Security.Cryptography.CryptoStream(inStream, transform, System.Security.Cryptography.CryptoStreamMode.Read))
      {
        byte[] buffer = new byte[4096];
        int bytesRead = cryptStream.Read(buffer, 0, buffer.Length);
        while (bytesRead > 0)
        {
          output.Write(buffer, 0, bytesRead);
          bytesRead = cryptStream.Read(buffer, 0, buffer.Length);

        }
      }
    }

    public Hashtable GetOptions(string name)
    {
      var options = new Hashtable();
      if (name == "Notification sending frequency")
      {
        foreach (NotificationFrequency frequencyLevel in Enum.GetValues(typeof(NotificationFrequency)))
        {
          options.Add(((int)frequencyLevel).ToString(), GetNotificationFrequencyText(frequencyLevel));
        }
      }
      else if (name == "Order state after export succeeded" || name == "Order state after export failed")
      {
        options.Add("", "Leave unchanged");
        foreach (var state in OrderState.GetAllOrderStates())
        {
          options.Add(state.Id, state.Name);
        }
      }
      else if (name == "Shop")
      {
        options.Add("", "Any");
        var shops = Shop.GetShops();
        foreach (var shop in shops)
        {
          options.Add(shop.Id, shop.Name);
        }
      }
      else if (name == "Cart communication type")
      {
        options.Add(Constants.CartCommunicationType.None, Constants.CartCommunicationType.None);
        options.Add(Constants.CartCommunicationType.Full, Constants.CartCommunicationType.Full);
        options.Add(Constants.CartCommunicationType.OnlyOnOrderComplete, Constants.CartCommunicationType.OnlyOnOrderComplete);
      }
      else if (name == "Synchronize on event")
      {
        options.Add(Constants.UserCommunicationType.None, Constants.UserCommunicationType.None);
        options.Add(Constants.UserCommunicationType.Full, Constants.UserCommunicationType.Full);
        options.Add(Constants.UserCommunicationType.OrderSubmit2NewUsers, Constants.UserCommunicationType.OrderSubmit2NewUsers);
        options.Add(Constants.UserCommunicationType.OrderSubmitAlways, Constants.UserCommunicationType.OrderSubmitAlways);
      }
      else if (name == "Type of user to sync")
      {
        options.Add(Constants.UserSyncType.None, Constants.UserSyncType.None);
        options.Add(Constants.UserSyncType.All, Constants.UserSyncType.All);
        options.Add(Constants.UserSyncType.LoginUsers, Constants.UserSyncType.LoginUsers);
        options.Add(Constants.UserSyncType.ImpersonateUsers, Constants.UserSyncType.ImpersonateUsers);
      }
      else
      {
        foreach (var cacheLevel in Enum.GetNames(typeof(CacheLevel)))
        {
          options.Add(cacheLevel, cacheLevel);
        }
      }
      return options;
    }

    private static string GetNotificationFrequencyText(NotificationFrequency frequency)
    {
      string result;
      switch (frequency)
      {
        case NotificationFrequency.Minute:
          result = "1 minute";
          break;
        case NotificationFrequency.FiveMinutes:
          result = "5 minutes";
          break;
        case NotificationFrequency.TenMinutes:
          result = "10 minutes";
          break;
        case NotificationFrequency.FifteenMinutes:
          result = "15 minutes";
          break;
        case NotificationFrequency.ThirtyMinutes:
          result = "30 minutes";
          break;
        case NotificationFrequency.OneHour:
          result = "1 hour";
          break;
        case NotificationFrequency.TwoHours:
          result = "2 hours";
          break;
        case NotificationFrequency.ThreeHours:
          result = "3 hours";
          break;
        case NotificationFrequency.SixHours:
          result = "6 hours";
          break;
        case NotificationFrequency.TwelveHours:
          result = "12 hours";
          break;
        case NotificationFrequency.OneDay:
          result = "1 day";
          break;
        default:
          result = "Never";
          break;
      }
      return result;
    }
  }
}
