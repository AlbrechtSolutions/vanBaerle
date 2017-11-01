using System;
using System.Web;
using Dna.Ecommerce.LiveIntegration.Addin;
using Dna.Ecommerce.LiveIntegration.Configuration;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Security.UserManagement;
using Dynamicweb.Security.UserManagement.Common.CustomFields;

namespace Dna.Ecommerce.LiveIntegration
{
	public class UrlHandler
	{
		private static UrlHandler _instance;
		private static readonly object SyncRoot = new object();

		private UrlHandler() { }

		public static UrlHandler Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (SyncRoot)
					{
						if (_instance == null)
							_instance = new UrlHandler();
					}
				}
				return _instance;
			}
		}

		private string GetUrl(string url)
		{
			return url.Contains(";") ? url.Substring(0, url.IndexOf(";")) : url;
		}

		private string UrlCacheKey => string.Format("{0}WebServiceURI", Constants.AddInName);

	  public void ClearCachedUrl()
		{
		  if (HttpContext.Current.Items[UrlCacheKey] != null)
		  {
		    HttpContext.Current.Items.Remove(UrlCacheKey);
		  }
		}

		public string GetWebServiceUrl()
		{
			string ret = string.Empty;
			if (HttpContext.Current.Items[UrlCacheKey] != null)
			{
				ret = (string)HttpContext.Current.Items[UrlCacheKey];
			}
			else
			{
				string multiLineUrlText = Settings.Instance.WebServiceURI;
				string[] urls = GetWebServiceUrls(multiLineUrlText);
				if (urls.Length > 0)
				{
					if (multiLineUrlText.Contains(";"))
					{
						foreach (string url in urls)
						{
							string[] parts = url.Split(';');
							if (parts.Length > 1)
							{
								string uri = parts[0];
								string fieldName = parts[1];
								string fieldValue = parts[2];
								if (!string.IsNullOrEmpty(fieldName))
								{
									if (fieldName.StartsWith("User."))
									{
									  if (TreatUserFields(fieldName, fieldValue))
									  {
									    ret = GetUrl(url);
									    break;
									  }
									  else
									  {
									    continue;
									  }
									}
									else if (fieldName.StartsWith("Order."))
									{
										if (Dynamicweb.Ecommerce.Common.Context.Cart != null)
										{
											foreach (OrderFieldValue ofv in Dynamicweb.Ecommerce.Common.Context.Cart.OrderFieldValues)
											{
												if (ofv != null && ofv.OrderField != null
													&& ofv.OrderField.SystemName == fieldName.Substring(6)
													&& (string)ofv.Value == fieldValue)
												{
													ret = GetUrl(url);
													break;
												}
											}
											if (!string.IsNullOrEmpty(ret))
												break;
										}
										else
											continue;
									}
									else if (fieldName == "Session.Shop" 
										&& Dynamicweb.Frontend.PageView.Current() != null 
										&& Dynamicweb.Frontend.PageView.Current().Area != null 
										&& fieldValue == Dynamicweb.Frontend.PageView.Current().Area.EcomShopId)
									{
										ret = GetUrl(url);
										break;
									}
								}
							}
						}
					}
					else
					{
						ret = urls[0];
					}
				}
				HttpContext.Current.Items[UrlCacheKey] = ret;
			}
			return ret;
		}

		private bool TreatUserFields(string fieldName, string fieldValue)
		{
			User user = User.GetCurrentExtranetUser();

			if (user == null)
				return false;

			if ((fieldName == "User.Company" && user.Company == fieldValue) ||
				(fieldName == "User.Department" && user.Department == fieldValue))
				return true;
			else
			{
				foreach (CustomFieldValue cfv in user.CustomFieldValues)
				{
					// fieldName.Substring: Remove the "User." prefix to
					// obtain system name only
					if (cfv != null && cfv.CustomField != null
						&& cfv.CustomField.SystemName == fieldName.Substring(5)
						&& (string)cfv.Value == fieldValue)
						return true;
				}
			}

			return false;
		}

		private string[] GetWebServiceUrls(string multilineUrlText)
		{
			string[] ret;

			if (!string.IsNullOrEmpty(multilineUrlText))
			{
				ret = new string[] { multilineUrlText };
				if (multilineUrlText.Contains("\r\n") || multilineUrlText.Contains("\n"))
				{
					ret = multilineUrlText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
				}
			}
			else
			{
				ret = new string[] { };
			}
			return ret;
		}
	}
}