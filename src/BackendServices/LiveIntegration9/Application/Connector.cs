using System;
using System.Web;
using System.Xml;
using Dna.Ecommerce.LiveIntegration.Addin;
using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.Exceptions;
using Dna.Ecommerce.LiveIntegration.Logging;
using Dynamicweb.Core;
using Dynamicweb.DataIntegration.Integration.ERPIntegration;

namespace Dna.Ecommerce.LiveIntegration
{
  internal static class Connector
  {
    private static DateTime? _lastErpCommunication;
    private static bool _isWebServiceConnectionAvailable;

    public static string Url
    {
      get
      {
        string url = UrlHandler.Instance.GetWebServiceUrl();
        if (string.IsNullOrEmpty(url))
        {
          if (string.IsNullOrEmpty(Settings.Instance.WebServiceURI))
          {
            throw new ArgumentException("Setup does not contain a url for the WebService. Please rerun setup.");
          }
          else
          {
            throw new ArgumentException("Cannot find appropriate web service url. Check Web Service Url settings.");
          }
        }
        return url;
      }
    }

    public static string SecurityKey => Settings.Instance.SecurityKey;

    public static XmlDocument CalculateOrder(string dwOrderXml, string orderId, bool createOrder)
    {
      XmlDocument result = null;

      if (IsWebServiceConnectionAvailable())
      {
        try
        {
          Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Request CalculateOrder sent. ID: {0}. CreateOrder: {1}. XML:\r\n{2}\r\n", orderId, createOrder, dwOrderXml));

          string erpOrderXmlResponse = ErpServiceCaller.GetDataFromRequestString(Url, SecurityKey, dwOrderXml);

          _lastErpCommunication = DateTime.Now;

          if (!string.IsNullOrEmpty(erpOrderXmlResponse))
          {
            if (!Helpers.ParseResponseToXml(erpOrderXmlResponse, out result))
            {
              result = null;
            }
            Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Response CalculateOrder received. ID: {0}. CreateOrder: {1}. XML:\r\n{2}\r\n", orderId, createOrder, erpOrderXmlResponse));
          }
          else
          {
            Logger.Instance.Log(ErrorLevel.ResponseError, "Response CalculateOrder returned null.");
          }
        }
        catch (Exception ex)
        {
          _lastErpCommunication = null;
          var errorMessage = string.Format("An error occurred while calling CalculateOrder from Web Service: '{0}'.", ex.Message);
          throw new LiveIntegrationException(errorMessage, ex);
        }
      }
      return result;
    }

    public static XmlDocument GetProductsInfo(string dwProductsXml)
    {
      XmlDocument result = null;

      if (IsWebServiceConnectionAvailable())
      {
        try
        {
          Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Request GetProductsInfo sent: '{0}'.", dwProductsXml));
          string erpProductsResponse = ErpServiceCaller.GetDataFromRequestString(Url, SecurityKey, dwProductsXml);

          _lastErpCommunication = DateTime.Now;

          if (!string.IsNullOrEmpty(erpProductsResponse))
          {
            if (!Helpers.ParseResponseToXml(erpProductsResponse, out result))
              result = null;

            Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Response GetProductsInfo received: '{0}'.", erpProductsResponse));
          }
          else
          {
            Logger.Instance.Log(ErrorLevel.ResponseError, "Response GetProductsInfo returned null.");
          }
        }
        catch (Exception ex)
        {
          _lastErpCommunication = null;
          Logger.Instance.Log(ErrorLevel.ConnectionError, string.Format("Error occurred while calling GetProductsInfo from Web Service: '{0}'.", ex.Message));
        }
      }
      return result;
    }

    public static XmlDocument RetrieveDataFromRequestString(string request)
    {
      XmlDocument result = null;
      if (IsWebServiceConnectionAvailable())
      {
        try
        {
          Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Request RetrieveDataFromRequestString sent: '{0}'.", request));
          string response = ErpServiceCaller.GetDataFromRequestString(Url, SecurityKey, request);

          _lastErpCommunication = DateTime.Now;

          if (!string.IsNullOrEmpty(response))
          {
            if (!Helpers.ParseResponseToXml(response, out result))
              result = null;

            Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Response GetProductsInfo received: '{0}'.", response));
          }
          else
          {
            Logger.Instance.Log(ErrorLevel.ResponseError, "Response GetProductsInfo returned null.");
          }
        }
        catch (Exception ex)
        {
          _lastErpCommunication = null;
          Logger.Instance.Log(ErrorLevel.ConnectionError, string.Format("Error in RetrieveDataFromRequestString: '{0}'. Request: '{1}'.", ex.Message, request));
        }
      }
      return result;
    }

    public static bool IsWebServiceConnectionAvailable()
    {
      if (_lastErpCommunication.HasValue && DateTime.Now.Subtract(_lastErpCommunication.Value).Minutes < 5)

      {
        return _isWebServiceConnectionAvailable;
      }

      bool success = false;
      //check if the connection status was already checked
      if (HttpContext.Current.Items[Constants.WebServiceConnectionCacheKey] != null)
      {
        success = Converter.ToBoolean(HttpContext.Current.Items[Constants.WebServiceConnectionCacheKey]);
      }
      else
      {
        const string request = "<GetEcomData></GetEcomData>"; //simple request for checking connection
        try
        {
          ErpServiceCaller.GetDataFromRequestString(Url, SecurityKey, request);
          success = true;
        }
        catch (Exception ex)
        {
          Logger.Instance.Log(ErrorLevel.ConnectionError, string.Format("Error checking Web Service connection: '{0}'. Request: '{1}'.", ex.Message, request));
        }

        //cache web service connection status to be available during one request
        HttpContext.Current.Items[Constants.WebServiceConnectionCacheKey] = success;
      }

      _isWebServiceConnectionAvailable = success;
      _lastErpCommunication = DateTime.Now;
      return success;
    }

	public static void ClearCache()
	{ 
		_lastErpCommunication = null;
	}

    public static XmlDocument UpdateUser(string dwUserXml)
    {
      XmlDocument result = null;

      if (IsWebServiceConnectionAvailable())
      {
        try
        {
          Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Request UpdateUser sent: '{0}'.", dwUserXml));

          string erpUserXmlResponse = ErpServiceCaller.GetDataFromRequestString(Url, SecurityKey, dwUserXml);

          _lastErpCommunication = DateTime.Now;

          if (!string.IsNullOrEmpty(erpUserXmlResponse))
          {
            if (!Helpers.ParseResponseToXml(erpUserXmlResponse, out result))
            {
              result = null;
            }

            Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Response UpdateUser received: '{0}'.", erpUserXmlResponse));
          }
          else
          {
            Logger.Instance.Log(ErrorLevel.ResponseError, "Response UpdateUser returned null.");
          }
        }
        catch (Exception ex)
        {
          _lastErpCommunication = null;
          Logger.Instance.Log(ErrorLevel.ConnectionError, string.Format("An error occurred while calling UpdateUser from Web Service: '{0}'.", ex.Message));
        }
      }
      return result;
    }
  }
}