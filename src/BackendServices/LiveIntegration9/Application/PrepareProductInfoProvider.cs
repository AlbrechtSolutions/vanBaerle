using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.Logging;
using Dynamicweb.Ecommerce.Integration;
using Dynamicweb.Ecommerce.International;
using Dynamicweb.Ecommerce.Prices;
using Dynamicweb.Ecommerce.Products;
using Dynamicweb.Frontend;
using Dynamicweb.Security.UserManagement;
using Dna.Ecommerce.LiveIntegration.Extensions;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dna.Ecommerce.LiveIntegration.XmlRendering.Renderers;
using Dna.Ecommerce.LiveIntegration.Addin;
using Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings;
using Dynamicweb.Ecommerce.Common;

namespace Dna.Ecommerce.LiveIntegration
{
    public class PrepareProductInfoProvider : PriceProvider
    {
        private class FetchProductInfoResult
        {
            public DateTime Executed = DateTime.Now;
            public bool Response;
        }
        private static readonly Dictionary<int, FetchProductInfoResult> FetchPageCache = new Dictionary<int, FetchProductInfoResult>();

        internal static CacheLevel ProductCacheLevel
        {
            get
            {
                string cacheLevelString = Settings.Instance.ProductCacheLevel;
                return Helpers.GetEnumValueFromString<CacheLevel>(cacheLevelString, CacheLevel.Page);
            }
        }

        private static bool AddProductFieldsToRequest => Settings.Instance.AddProductFieldsToRequest;

        public override PriceRaw FindPrice(Product product, double quantity, string variantId, Currency currency, string unitId, User user)
        {
            if (!Global.IsIntegrationActive
              || !Settings.Instance.EnableLivePrices
              || !Settings.Instance.LiveProductInfoForAnonymousUsers && user == null
              || user != null && user.IsLivePricesDisabled
              || !Connector.IsWebServiceConnectionAvailable()
              || product == null || product.Id == "" || product.Number == "" || product.Number  == null)
                return null;

            try
            {
                Product requestProduct;
                if (product.VariantId == variantId || (string.IsNullOrEmpty(variantId) && string.IsNullOrEmpty(product.VariantId)))
                {
                    requestProduct = product;
                }
                else
                {
                    requestProduct = product.Clone();
                    requestProduct.VariantId = variantId;
                }

                //Logger.Instance.Log(ErrorLevel.DebugInfo, $"Id: {requestProduct.Id}, VariantId: {requestProduct.VariantId}, Number: {requestProduct.Number} \r\n { System.Environment.StackTrace}");

                // If Product is not in the cache, then get it from Erp
                if (!ErpResponseCache.IsProductInCache(ProductCacheLevel, Helpers.ProductIdentifier(requestProduct)))
                {
                    List<Product> products = new List<Product>();
                    products.Add(requestProduct);
                    if (FetchProductInfos(products, User.GetCurrentExtranetUser()))
                    {
                        //Check if requested product was not received from response
                        if (!ErpResponseCache.IsProductInCache(ProductCacheLevel, Helpers.ProductIdentifier(requestProduct)))
                        {
                            Logger.Instance.Log(ErrorLevel.Error, string.Format("Error receiving product info for product: {0}.", Helpers.ProductIdentifier(requestProduct)));
                            return null;
                        }
                    }
                    else
                    {
                        //no price from ERP so read DW default price
                        return new DefaultPriceProvider().FindPrice(product, quantity, variantId, currency, unitId, user);
                    }
                }

                double? priceValue = 0.0;

                var productCache = ErpResponseCache.GetProductInfos(ProductCacheLevel);

                ProductInfo productInfo = productCache != null && productCache.TryGetValue(Helpers.ProductIdentifier(requestProduct), out productInfo)
                    ? productInfo
                    : null;

                if (productInfo != null)
                {
                    priceValue = SelectPrice(productInfo, quantity);
                }

                var price = priceValue == null
                    ? null
                    : new PriceRaw
                        {
                            Price = priceValue.Value * product.GetUnitPriceMultiplier(),
                            Currency = currency
                        };

                return price;
            }
            catch (Exception e)
            {
                Logger.Instance.Log(ErrorLevel.Error, $"Unknown error during FindPrice(). Exception: {e.Message}");
                return null;
            }
        }


        //protected virtual PriceRaw GetDefaultPrice(Product product, Currency currency)
        //{
        //    return new PriceRaw
        //    {
        //        Price = product.DefaultPrice * product.GetUnitPriceMultiplier().GetValueOrDefault(1),
        //        Currency = currency
        //    };
        //}


        public override void PreparePrices(Dictionary<Product, double> products)
        {
            // This overload is invoked when OrderLines are prepared (only).
            return;
        }

        public override void PreparePrices(ProductCollection products)
        {
            // This overload is invoked when products are rendered.

            if (products != null
              && Connector.IsWebServiceConnectionAvailable()
              && Global.IsIntegrationActive
              && Global.IsIntegrationEnabledForShop)
            {
                FetchProductInfos(products.Where(p => !string.IsNullOrEmpty(p.Number)).ToList());
            }
        }

        internal static bool FetchProductInfos(List<Product> products, User user)
        {
            // disable because if causes troubles if the product list fails (missing product) and then we enter to a product, as
            // it won't fetch its price...
            //if (LastResponseValid())
            //    // no need to read cache
            //    return false;
            //SaveLastResponse(false);

            if (products == null || products.Count == 0)
                return false;

            if (!Settings.Instance.EnableLivePrices)
            {
                Logger.Instance.Log(ErrorLevel.DebugInfo, "Live Prices are Disabled");
                return false;
            }

            if (!Connector.IsWebServiceConnectionAvailable())
            {
                Logger.Instance.Log(ErrorLevel.DebugInfo, "Live Prices are unavailable");
                return false;
            }

            if (user == null)
            {
                user = User.GetCurrentExtranetUser();
            }
            if (!Settings.Instance.LiveProductInfoForAnonymousUsers && user == null)
            {
                Logger.Instance.Log(ErrorLevel.DebugInfo, "No user is currently logged in. Anonymous user is not allowed.");
                return false;
            }

            if (user != null && user.IsLivePricesDisabled)
            {
                Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Calculated prices are not allowed to the user '{0}'.", user.UserName));
                return false;
            }

            // Check for existence of the given products in the Cache
            var productsForRequest = new List<Product>();
            foreach (var product in products)
            {
                // check for invalid products (without product id)
                if (string.IsNullOrEmpty(product.Id))
                {
                    Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Live Prices for invalid product: [{0}]", product.PropertiesToString()));
                }
                // Check for existence of the given products in the Cache
                else if (!ErpResponseCache.IsProductInCache(ProductCacheLevel, Helpers.ProductIdentifier(product)))
                {
                    productsForRequest.Add(product);
                }
            }

            if (productsForRequest.Count == 0)
            {
                //Logger.Instance.Log(ErrorLevel.DebugInfo, "All products in cache!");
                SaveLastResponse();
                return true;
            }

            var request = BuildProductRequest(productsForRequest);

            var response = Connector.GetProductsInfo(request);
            if (!string.IsNullOrEmpty(response?.InnerXml))
            {
                // Parse the response
                Dictionary<string, ProductInfo> prices = ProcessResponse(response);

                if (prices == null || prices.Count == 0)
                {
                    //Logger.Instance.Log(ErrorLevel.DebugInfo, "No result products!");
                    return false;
                }

                // Cache prices
                foreach (string productKey in prices.Keys)
                {
                    Dictionary<string, ProductInfo> piCache = ErpResponseCache.GetProductInfos(ProductCacheLevel);
                    if (piCache.ContainsKey(productKey))
                    {
                        piCache.Remove(productKey);
                    }
                    piCache.Add(productKey, prices[productKey]);
                }
            }
            else    //error occurred
            {
                return false;
            }

            SaveLastResponse();
            return true;
        }

        /// <summary>
        ///     Checks whether the cache dependency has been changed from the last request.
        ///     If the current request has a different signature that the previous one, the cache will be cleared. 
        /// </summary>
        public static void ClearContextDependentCache(int userId)
        {
            string cacheKeyInfoStore = "ContextDependentCacheKeyInfoStore";

            var session = System.Web.HttpContext.Current?.Session;

            string cacheKeyDependency = userId.ToString();
            string existingCacheKeyDependency = (string)session?[cacheKeyInfoStore];

            if (!string.Equals(cacheKeyDependency, existingCacheKeyDependency, StringComparison.OrdinalIgnoreCase))
            {
                ErpResponseCache.ClearAllCaches();

                if (session != null)
                {
                    session[cacheKeyInfoStore] = cacheKeyDependency;
                }
            }
        }

        public static bool FetchProductInfos(List<Product> products)
        {
            return FetchProductInfos(products, null);
        }

        // Fills the values from Response on the Product
        public static void FillProductValues(Product product, double quantity = 1)
        {
            if (product != null && ErpResponseCache.IsProductInCache(ProductCacheLevel, Helpers.ProductIdentifier(product)))
            {
                Dictionary<string, ProductInfo> piCache = ErpResponseCache.GetProductInfos(ProductCacheLevel);
                if (piCache.ContainsKey(Helpers.ProductIdentifier(product)))
                {
                    ProductInfo productInfo = piCache[Helpers.ProductIdentifier(product)];

                    double? newStock = (double?)productInfo["Stock"];

                    if (newStock.HasValue && product.Stock != newStock)
                    {
                        product.Stock = newStock.Value;
                        product.Save();
                        productInfo["Stock"] = null;
                    }
                    //product.Stock = (double)productInfo["Stock"];

                    FillProductPrices(product, productInfo);

                    double? priceValue = SelectPrice(productInfo, quantity) * product.GetUnitPriceMultiplier();

                    product.Price.PriceWithoutVAT = priceValue.GetValueOrDefault(); // NOTE accessing the Price property, will trigger the price provider to kick in
                    product.Price.PriceWithVAT = priceValue.GetValueOrDefault();

                    //Update Product Custom Fields
                    if (AddProductFieldsToRequest && product.ProductFieldValues.Count > 0)
                    {
                        foreach (var pfv in product.ProductFieldValues)
                        {
                            if (productInfo.ContainsKey(pfv.ProductField.SystemName))
                                pfv.Value = productInfo[pfv.ProductField.SystemName];
                        }
                    }
                }
            }
        }

        private static void FillProductPrices(Product product, ProductInfo productInfo)
        {
            var prices = (IList<ProductPrice>)productInfo["Prices"];

            foreach (var price in prices.OrderBy(p => p.Quantity.GetValueOrDefault()))
            {
                product.Prices.Add(new Price
                {
                    Id = price.Id,
                    Amount = price.Amount.GetValueOrDefault() * product.GetUnitPriceMultiplier(),
                    Quantity = price.Quantity.GetValueOrDefault(),
                    ProductId = price.ProductId,
                    VariantId = price.ProductVariantId,
                    LanguageId = Context.LanguageID,
                    UserCustomerNumber = price.UserCustomerNumber,
                    IsInformative = false,
                    CurrencyCode = Context.Currency.Code
                });
            }
        }

        #region Fetch response cache
        internal static void ClearUserCache()
        {
            if (ProductCacheLevel == CacheLevel.Session)
            {
                System.Web.HttpContext.Current.Session.Remove(SessionCacheKey());
            }
            else
            {
                FetchPageCache.Clear();
            }
        }

        private static string SessionCacheKey()
        {
            var user = User.GetCurrentExtranetUser();
            var sSessionCache = Constants.CacheConfiguration.FetchProductInfoResult;

            if (user != null)
            {
                sSessionCache += user.Name;
            }
            else
            {
                sSessionCache += Settings.Instance.AnonymousUserKey;
            }
            return sSessionCache;
        }

        private static bool LastResponseValid()
        {
            FetchProductInfoResult fetchResult = null;
            if (ProductCacheLevel == CacheLevel.Session)
            {
                object oAux = System.Web.HttpContext.Current.Session[SessionCacheKey()];
                if (oAux != null)
                {
                    fetchResult = oAux as FetchProductInfoResult;
                }
            }
            else
            {
                var pageId = PageView.Current().ID;
                if (FetchPageCache.ContainsKey(pageId))
                {
                    fetchResult = FetchPageCache[pageId];
                }
            }

            if (fetchResult != null && !fetchResult.Response && fetchResult.Executed.Subtract(DateTime.Now).Minutes < 5)
                return true;

            return false;
        }

        private static void SaveLastResponse(bool fetchResponse = true)
        {
            FetchProductInfoResult fetchResult = new FetchProductInfoResult
            {
                Response = fetchResponse
            };


            if (ProductCacheLevel == CacheLevel.Session)
            {
                System.Web.HttpContext.Current.Session[SessionCacheKey()] = fetchResult;
            }
            else
            {
                var pageId = PageView.Current().ID;
                if (FetchPageCache.ContainsKey(pageId))
                {
                    FetchPageCache[pageId] = fetchResult;
                }
                else
                {
                    FetchPageCache.Add(pageId, fetchResult);
                }
            }
        }
        #endregion Fetch response cache

        #region Build request and parse response

        private static string BuildProductRequest(List<Product> products)
        {
            return new ProductInfoXmlRenderer().RenderProductInfoXml(products,
              new RenderProductInfoSettings
              {
                  AddProductFieldsToRequest = AddProductFieldsToRequest,
                  Beautify = false,
                  LiveIntegrationSubmitType = LiveIntegrationSubmitType.Live,
                  ReferenceName = "ProductInfoLive"
              });
        }

        /// <summary>
        /// Processes the response with product info.
        /// </summary>
        private static Dictionary<string, ProductInfo> ProcessResponse(XmlDocument response)
        {
            if (response == null)
                return null;

            try
            {
                DateTime now = DateTime.Now;
                string language = Context.LanguageID;
                Dictionary<string, ProductInfo> infos = new Dictionary<string, ProductInfo>();

                Func<string, string, string> priceLookupKey = (productId, variantId) => string.Join("|", productId, variantId);
                ILookup<string, ProductPrice> pricesLookup = ExtractPrices(response).ToLookup(p => priceLookupKey(p.ProductId, p.ProductVariantId), StringComparer.OrdinalIgnoreCase);
                foreach (XmlNode item in response.SelectNodes("//item [@table='EcomProducts']"))
                {
                    string productId = item.SelectSingleNode("column [@columnName='ProductId']").InnerText;
                    string variantId = item.SelectSingleNode("column [@columnName='ProductVariantId']").InnerText;

                    string productKey = Helpers.ProductIdentifier(productId, variantId, language, item.SelectSingleNode("column [@columnName='ProductNumber']").InnerText);

                    ProductInfo productInfo = new ProductInfo();
                    // Set ProductId
                    productInfo["ProductId"] = productKey;
                    // Set price
                    productInfo["TotalPrice"] = Helpers.ReadDouble(item.SelectSingleNode("column [@columnName='ProductPrice']").InnerText);
                    // Set stock
                    // Set prices
                    productInfo["Prices"] = pricesLookup[priceLookupKey(productId, variantId)].ToList();
                    var stock = item.SelectSingleNode("column [@columnName='ProductStock']").InnerText;
                    if (stock != null)
                    {
                        productInfo["Stock"] = Helpers.ReadDouble(stock);
                    }
                    else
                    {
                        productInfo["Stock"] = null;
                    }
                    //Set ProductCustomFields
                    if (AddProductFieldsToRequest)
                    {
                        XmlNode fieldNode = null;
                        foreach (ProductField pf in ProductField.GetProductFields())
                        {
                            fieldNode = item.SelectSingleNode(string.Format("column [@columnName='{0}']", pf.SystemName));
                            if (fieldNode != null)
                            {
                                productInfo[pf.SystemName] = fieldNode.InnerText;
                            }
                        }
                    }

                    // avoid exception to duplicate products in XML
                    if (!infos.ContainsKey(productKey))
                        infos.Add(productKey, productInfo);
                }

                return infos;
            }
            catch (Exception e)
            {
                Logger.Instance.Log(ErrorLevel.Error, string.Format("Response does not match schema: '{0}'.", e.Message));
            }

            return null;
        }

        private static IList<ProductPrice> ExtractPrices(XmlDocument response)
        {
            var prices = new List<ProductPrice>();

            var pricesXml = response.SelectNodes("//item [@table='EcomPrices']");

            if (pricesXml != null)
            {
                var culture = CultureInfo.InvariantCulture;

                foreach (XmlNode priceXml in pricesXml)
                {
                    var price = new ProductPrice
                    {
                        Id = priceXml.SelectSingleNode("column [@columnName='PriceId']")?.InnerText,
                        ProductId = priceXml.SelectSingleNode("column [@columnName='PriceProductId']")?.InnerText,
                        ProductVariantId = priceXml.SelectSingleNode("column [@columnName='PriceProductVariantId']")?.InnerText,
                        UserCustomerNumber = priceXml.SelectSingleNode("column [@columnName='PriceUserCustomerNumber']")?.InnerText
                    };


                    double amount;
                    price.Amount = double.TryParse(priceXml.SelectSingleNode("column [@columnName='PriceAmount']")?.InnerText, NumberStyles.Any, culture, out amount) ? amount : (double?)null;

                    double quantity;
                    price.Quantity = double.TryParse(priceXml.SelectSingleNode("column [@columnName='PriceQuantity']")?.InnerText, NumberStyles.Any, culture, out quantity) ? quantity : (double?)null;

                    DateTime validFrom;
                    price.ValidFrom = DateTime.TryParse(priceXml.SelectSingleNode("column [@columnName='PriceValidFrom']")?.InnerText, culture, DateTimeStyles.AssumeLocal, out validFrom) ? validFrom : (DateTime?)null;

                    DateTime validTo;
                    price.ValidTo = DateTime.TryParse(priceXml.SelectSingleNode("column [@columnName='PriceValidTo']")?.InnerText, culture, DateTimeStyles.AssumeLocal, out validTo) ? validTo : (DateTime?)null;

                    prices.Add(price);
                }
            }

            return prices;
        }

        public static double? SelectPrice(ProductInfo product, double quantity)
        {
            double? price = null;

            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            quantity = Math.Max(1, quantity);

            var now = DateTime.Now;

            var erpPriceResponse = ((IList<ProductPrice>)product["Prices"] ?? Enumerable.Empty<ProductPrice>())
                   .Where(p =>
                        (p.ValidFrom == null || p.ValidFrom <= now) &&
                        (p.ValidTo == null || p.ValidTo >= now) &&
                        quantity >= p.Quantity.GetValueOrDefault(1)
                    )
                   .OrderByDescending(p => p.Quantity.GetValueOrDefault(1))
                   .FirstOrDefault();

            price = erpPriceResponse?.Amount;

            if (price == null)
            {
                price = (double)product["TotalPrice"];
            }

            return price;
        }
        #endregion
    }
}