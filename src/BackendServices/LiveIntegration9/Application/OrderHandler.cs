using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using Dna.Ecommerce.LiveIntegration.Addin;
using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.Exceptions;
using Dna.Ecommerce.LiveIntegration.Extensions;
using Dna.Ecommerce.LiveIntegration.ExtensionsMethods;
using Dna.Ecommerce.LiveIntegration.Logging;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dna.Ecommerce.LiveIntegration.XmlRendering.Renderers;
using Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings;
using Dynamicweb.Configuration;
using Dynamicweb.Core;
using Dynamicweb.Data;
using Dynamicweb.Ecommerce.Integration;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Ecommerce.Prices;
using Dynamicweb.Environment;
using Dynamicweb.Security.UserManagement;

namespace Dna.Ecommerce.LiveIntegration
{
  internal static class OrderHandler
  {
    private static CacheLevel OrderCacheLevel
    {
      get
      {
        string cacheLevelString = Settings.Instance.OrderCacheLevel;
        return Helpers.GetEnumValueFromString(cacheLevelString, CacheLevel.Page);
      }
    }

    private static bool AddOrderFieldsToRequest => Settings.Instance.AddOrderFieldsToRequest;

    private static bool AddOrderLineFieldsToRequest => Settings.Instance.AddOrderLineFieldsToRequest;
    private static bool SaveOrderXml => Settings.Instance.SaveCopyOfOrderXml;

    private const string OrderXmlLogFolder = "/Files/System/Log/LiveIntegration/OrderXml";

    /// <summary>
    /// Updates an order in the ERP. 
    /// </summary>
    /// <param name="order">The order that must be synced with the ERP.</param>
    /// <param name="liveIntegrationSubmitType">Determines the origin of this submit such.</param>
    /// <param name="successOrderStateId">The order state that is applied to the order when it integrates successfully.</param>
    /// <param name="failedOrderStateId">The order state that is applied to the order when an error occurred during the integration.</param>
    /// <returns>Returns null if no communication has made, or bool if order has been updated successfully or not.</returns>
    public static bool? UpdateOrder(Order order, LiveIntegrationSubmitType liveIntegrationSubmitType, string successOrderStateId = null, string failedOrderStateId = null)
    {
      if (order == null || !order.OrderLines.Any())
      {
        return null;
      }
      if (liveIntegrationSubmitType == LiveIntegrationSubmitType.LiveOrderOrCart && !string.IsNullOrEmpty(order.IntegrationOrderId))
      {
        return null;
      }

      var orderId = order.Id ?? "ID is null";
      Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Updating order with ID: {0}. Complete: {1}. Order submitted from the backend: {2}. Stack trace: {3}", orderId, order.Complete, ExecutingContext.IsBackEnd(), Environment.StackTrace));

      // use current user if is not backend running or if the cart is Anonymous
      var user = User.GetUserByID(order.CustomerAccessUserId) ?? (!ExecutingContext.IsBackEnd() ? User.GetCurrentExtranetUser() : null);

      /* create order: if it is false, you will get a calculate order from the ERP with the total prices */
      /* if it is true, then a new order will be created in the ERP */
      bool createOrder = order.Complete;

      if (createOrder && user != null)
      {
        UserSync(user);
      }

      if (!Settings.Instance.EnableCartCommunicationForAnonymousUsers && user == null)
      {
        Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("No user is currently logged in. Anonymous user cart is not allowed. Order = {0}", orderId));
        return null;
      }

      // default states
      if (successOrderStateId == null)
      {
        successOrderStateId = Settings.Instance.OrderStateAfterExportSucceeded;
      }
      if (failedOrderStateId == null)
      {
        failedOrderStateId = Settings.Instance.OrderStateAfterExportFailed;
      }

      var requestXml = new OrderXmlRenderer().RenderOrderXml(order, new RenderOrderSettings { AddOrderLineFieldsToRequest = AddOrderLineFieldsToRequest, AddOrderFieldsToRequest = AddOrderFieldsToRequest, CreateOrder = createOrder, LiveIntegrationSubmitType = liveIntegrationSubmitType, ReferenceName = "OrdersPut" });

      if (createOrder && SaveOrderXml && liveIntegrationSubmitType == LiveIntegrationSubmitType.LiveOrderOrCart)
      {
        SaveCopyOfXml(order.Id, requestXml);
      }

      // calculate current hash
      string currentHash = CalculateHash(requestXml);

      // get last hash
      string lastHash = GetLastOrderHash();

      if (!string.IsNullOrEmpty(lastHash) && lastHash == currentHash)
      {
        // no changes to order
        return null;
      }
      // save this hash for next calls
      SaveOrderHash(currentHash);

      // clear all session var to avoid miss calculations
      HttpContext.Current.Session.Remove("LiveIntegrationDiscounts" + order.Id);

      Dictionary<string, XmlDocument> responsesCache = ErpResponseCache.GetWebOrdersConnectorResponses(OrderCacheLevel);

      XmlDocument response = null;
      if (!createOrder && responsesCache.ContainsKey(requestXml))
      {
        response = responsesCache[requestXml];
      }
      else
      {
        if (createOrder)
        {
          OrderDebuggingInfo.Save(order, "Sending order to the ERP system through Live Integration.", "OrderHandler");
        }
        try
        {
          response = Connector.CalculateOrder(requestXml, order.Id, createOrder);
        }
        catch (LiveIntegrationException ex)
        {
          // TODO Rui, handle appropriately 
          Logger.Instance.Log(ErrorLevel.ConnectionError, ex.Message);
        }
        if (responsesCache.ContainsKey(requestXml))
        {
          responsesCache.Remove(requestXml);
        }
        if (!string.IsNullOrWhiteSpace(response?.InnerXml))
        {
          responsesCache.Add(requestXml, response);
        }
      }
      if (!string.IsNullOrWhiteSpace(response?.InnerXml))
      {
        bool processResponseResult = ProcessResponse(response, order, createOrder, successOrderStateId, failedOrderStateId);

        // new user sync active, sync user (Update)
        if (Settings.Instance.UpdateUserAfterNewOrder && user != null)
        {
          user.SynchronizeUsingLiveIntegration(true, UserSyncMode.Get);
        }

        return processResponseResult;
      }
      else
      {
        //error occurred
        CheckIfOrderShouldRevertToCart(order, createOrder);
        return false;
      }
    }

    /// <summary>
    /// Saves a copy of the order XML to a custom log folder under the main LiveIntegration log folder.
    /// </summary>
    /// <param name="orderId">The ID of the order being saved; used in the file name.</param>
    /// <param name="requestXml">The order XML to save.</param>
    private static void SaveCopyOfXml(string orderId, string requestXml)
    {
      try
      {
        var folder = GetLogFolderForXmlCopies();
        string file = BuildXmlCopyPath(orderId, folder);
        var doc = new XmlDocument();
        doc.LoadXml(requestXml);
        File.WriteAllText(file, doc.Beautify());
      }
      catch (Exception e)
      {
        Logger.Instance.Log(ErrorLevel.Error, "Error writing copy of XML to log folder: " + e.Message);
      }
    }

    internal static string BuildXmlCopyPath(string orderId, string folder)
    {
      return Path.Combine(folder, string.Format("{0}.xml", orderId));
    }

    internal static string GetLogFolderForXmlCopies(DateTime? date = null)
    {
      if (!date.HasValue)
      {
        date = DateTime.Now;
      }
      try
      {
        var logFolder = string.Format("{0}/{1}/{2:MM}", OrderXmlLogFolder, date.Value.Year, date.Value);
        var logFolderPhysical = HttpContext.Current.Server.MapPath(logFolder);
        if (!Directory.Exists(logFolderPhysical))
        {
          Directory.CreateDirectory(logFolderPhysical);
        }
        return logFolderPhysical;
      }
      catch (Exception e)
      {
        Logger.Instance.Log(ErrorLevel.Error, "Error creating log folder for order XML files: " + e.Message);
        return "";
      }
    }

    private static void UserSync(User user)
    {
      // check type of user synchronization
      string configValue = Settings.Instance.UserCommunicationType;

      if (configValue == Constants.UserCommunicationType.None)
      // nothing to do
      {
        return;
      }

      // case is only new users then
      if (configValue == Constants.UserCommunicationType.OrderSubmit2NewUsers && !string.IsNullOrEmpty(user.ExternalID))
      // user already synced
      {
        return;
      }

      // sync user
      user.SynchronizeUsingLiveIntegration();
    }

    private static bool ProcessResponse(XmlDocument response, Order order, bool createOrder, string successState, string failedState)
    {
      //        var orderParser = new OrderXmlParser(response);

     Logger.Instance.Log(ErrorLevel.DebugInfo, "Process response..."); // TODO: removeee
     

      var orderId = order == null ? "is null" : order.Id ?? "ID is null";

      if (response == null || order == null)
      {
        if (createOrder)
        {
          // if must create order and no response or invalid order fail to sync
          Logger.Instance.Log(ErrorLevel.Error, string.Format("Response CreateOrder is null. Order = {0}", orderId));
          return false;
        }

        // nothing to do so work done
        return true;
      }

      try
      {
        XmlNode orderNode = response.SelectSingleNode("//item [@table='EcomOrders']");
        if (Global.EnableCartCommunication(order.Complete))
        {
          // Set Order prices
          //order.AllowOverridePrices = true;
          try
          {
            //SetPrices(order, orderNode);

            SetCustomerNumber(order, orderNode);
          }
          catch (Exception ex)
          {
            Logger.Instance.Log(ErrorLevel.Error, string.Format("Exception setting prices: {0} Order = {1}", ex.Message, orderId));
          }
        }

        SetCustomOrderFields(order, orderNode);

        var discountOrderLines = new OrderLineCollection();

        if (Global.EnableCartCommunication(order.Complete))
        {
          ProcessOrderLines(response, order, discountOrderLines);
          //order.OrderLines.Add(discountOrderLines);
        }

        if (createOrder)
        {
          AssignIntegrationOrderId(order, orderNode);
          var orderCreatedSuccessfully = bool.Parse(orderNode.SelectSingleNode("column [@columnName='OrderCreated']").InnerText);
          if (!orderCreatedSuccessfully)
          {
            HandleIntegrationFailure(order, failedState, orderId, discountOrderLines);
          }
          else
          {
            HandleIntegrationSuccess(order, successState);
          }
        }
        //else
        //{
        //    order.Save();
        //}

        if (Global.EnableCartCommunication(order.Complete))
        {
          //add discount orderlines to cache, to be added by the discountProvider
          HttpContext.Current.Session["LiveIntegrationDiscounts" + order.Id] = discountOrderLines;
        }
      }
      catch (Exception e)
      {
        Logger.Instance.Log(ErrorLevel.Error, string.Format("Error processing response. Error: '{0}' Order = {1}.", e, orderId));
        return false;
      }

      return true;
    }

    private static void SetCustomerNumber(Order order, XmlNode orderNode)
    {
      var orderCustomerNumber = orderNode.SelectSingleNode("column [@columnName='OrderCustomerNumber']");
      if (orderCustomerNumber != null)
      {
        var newCustomerNumber = orderCustomerNumber.InnerText;

        if (string.Compare(order.CustomerNumber, newCustomerNumber, StringComparison.InvariantCultureIgnoreCase) != 0)
        {
          order.CustomerNumber = newCustomerNumber;
        }
      }
    }

    private static void CheckIfOrderShouldRevertToCart(Order order, bool createOrder)
    {
      if (createOrder && !Settings.Instance.QueueOrdersToExport)
      {
        order.DowngradeToCart();
        Dynamicweb.Ecommerce.Common.Context.SetCart(order);
        order.CartV2StepIndex = --order.CartV2StepIndex;
        order.Complete = false;
      }
    }

    private static void HandleIntegrationSuccess(Order order, string successState)
    {
      if (!string.IsNullOrWhiteSpace(successState))
      {
        order.StateId = successState;
      }

      order.Save();
      HttpContext.Current.Session["LiveIntegration.OrderExportFailed"] = null;
      HttpContext.Current.Session["LiveIntegration.FailedOrderId"] = null;
    }

    private static void HandleIntegrationFailure(Order order, string failedState, string orderId, OrderLineCollection discountOrderLines)
    {
      if (Global.EnableCartCommunication(order.Complete))
      {
        RemoveDiscounts(order);
        order.OrderLines.Add(discountOrderLines);
      }
      Logger.Instance.Log(ErrorLevel.Error, string.Format("Order with ID '{0}' was not created in the ERP system.", orderId));
      HttpContext.Current.Session["LiveIntegration.OrderExportFailed"] = true;
      HttpContext.Current.Session["LiveIntegration.FailedOrderId"] = order.Id;

      if (!Settings.Instance.QueueOrdersToExport)
      {
        order.DowngradeToCart();
        Dynamicweb.Ecommerce.Common.Context.SetCart(order);
        order.CartV2StepIndex = --order.CartV2StepIndex;
        order.Complete = false;
      }

      if (!string.IsNullOrWhiteSpace(failedState))
      {
        order.StateId = failedState;
      }

      order.Save();
    }

    private static void ProcessOrderLines(XmlDocument response, Order order, OrderLineCollection discountOrderLines)
    {
      Logger.Instance.Log(ErrorLevel.DebugInfo, "ProcessOrderLines..."); // TODO: removeee

      XmlNode orderNode = response.SelectSingleNode("//item [@table='EcomOrders']");
      var orderCreated = bool.Parse(orderNode.SelectSingleNode("column [@columnName='OrderCreated']").InnerText);

      XmlNodeList orderLinesNodes = response.SelectNodes("//item [@table='EcomOrderLines']");
      if (orderLinesNodes != null && orderLinesNodes.Count > 0)//Process OrderLines
      {
        List<string> orderlineIDs = new List<string>();
        List<OrderLine> orderLines = order.OrderLines.ToList();

        foreach (XmlNode orderLineNode in orderLinesNodes)
        {
          XmlNode xnOrderLineType = orderLineNode.SelectSingleNode("column [@columnName='OrderLineType']");

          if (xnOrderLineType == null)
          {
            xnOrderLineType = orderLineNode.SelectSingleNode("column [@columnName='OrderLineTypeId']");
          }

          string orderLineType = xnOrderLineType.InnerText;
          if (orderLineType == "0" || string.IsNullOrWhiteSpace(orderLineType))
          {
            Logger.Instance.Log(ErrorLevel.DebugInfo, "ProcessOrderLines 0 type..."); // TODO: removeee

            ProcessProductOrderLines(order, orderlineIDs, orderLines, orderLineNode, orderCreated);
          }
          if (orderLineType == "1" || orderLineType == "3") //1=order discount, 3=Product Discount
          {
            Logger.Instance.Log(ErrorLevel.DebugInfo, "ProcessOrderLines 1, 3 type..."); // TODO: removeee

            ProcessDiscountOrderLines(order, discountOrderLines, orderLineNode, orderLineType, orderCreated);
          }
        }

        // Remove deleted OrderLines
        for (int i = order.OrderLines.Count - 1; i >= 0; i--)
        {
          var orderLine = order.OrderLines[i];
          if (string.IsNullOrWhiteSpace(orderLine.Id))
          {
            continue;
          }
          if (orderLine.Type == "1" || orderLine.Type == "3") //1=order discount, 3=Product Discount
          {
            continue;
          }
          if (!orderlineIDs.Contains(orderLine.Id))
          {
            order.OrderLines.Remove(orderLine);
            orderLine.Delete();
          }
        }
      }
    }

    private static void SetCustomOrderFields(Order order, XmlNode orderNode)
    {
      //Set OrderCustomFields values
      if (!AddOrderFieldsToRequest || order.OrderFieldValues.Count <= 0)
      {
        return;
      }
      foreach (var orderFieldValue in order.OrderFieldValues)
      {
        var fieldNode = orderNode.SelectSingleNode(string.Format("column [@columnName='{0}']", orderFieldValue.OrderField.SystemName));
        if (fieldNode != null)
        {
          orderFieldValue.Value = fieldNode.InnerText;
        }
      }
    }

    private static void SetPrices(Order order, XmlNode orderNode)
    {
      OrderPriceCalculation(order, orderNode);
      var orderDiscount = orderNode.SelectSingleNode("column [@columnName='OrderSalesDiscount']");
      if (orderDiscount != null)
      {
        order.SalesDiscount = Helpers.ReadDouble(orderDiscount.InnerText);
      }
    }

    private static void AssignIntegrationOrderId(Order order, XmlNode orderNode)
    {
      // search for IntegrationOrderID field in response otherwise use the OrderIntegrationOrderID
      XmlNode integrationIdNode = orderNode.SelectSingleNode("column [@columnName='OrderIntegrationOrderId']");

      // otherwise use the traditional OrderID
      if (string.IsNullOrEmpty(integrationIdNode?.InnerText))
      {
        integrationIdNode = orderNode.SelectSingleNode("column [@columnName='OrderID']");
      }
      if (!string.IsNullOrWhiteSpace(integrationIdNode?.InnerText))
      {
        order.IntegrationOrderId = integrationIdNode.InnerText;
        order.IsExported = true;
      }
    }

    private static double? ReadDouble(XmlNode elementNode, string field)
    {
      XmlNode xValue = elementNode.SelectSingleNode(field);

      if (xValue == null)
      {
        return null;
      }

      return Helpers.ReadDouble(xValue.InnerText);
    }

    private static void ProcessProductOrderLines(Order order, List<string> orderlineIDs, List<OrderLine> orderLines, XmlNode orderLineNode, bool orderCreated)
    {
      Logger.Instance.Log(ErrorLevel.DebugInfo, "ProcessProductOrderLines..."); // TODO: removeee

      string productNumber = orderLineNode.SelectSingleNode("column [@columnName='OrderLineProductNumber']").InnerText;

      try
      {
        double? dAux;

        if (!string.IsNullOrWhiteSpace(productNumber))
        {
          OrderLine orderLine = orderLines.FirstOrDefault(ol => ol.ProductNumber == productNumber);
          if (orderLine != null)
          {
            orderLines.Remove(orderLine);
            Logger.Instance.Log(ErrorLevel.DebugInfo, "ProcessProductOrderLines removing..."); // TODO: removeee

            //Remove found line for getting next line with same ProductNumber
          }
          else // Create an OrderLine if it doesn't exist
          {
            orderLine = CreateOrderLine(order, productNumber);
            Logger.Instance.Log(ErrorLevel.DebugInfo, "ProcessProductOrderLines creating..."); // TODO: removeee

            if (orderLine == null)
            {
              return;
            }
          }
          if (!string.IsNullOrWhiteSpace(orderLine.Id))
          {
            orderlineIDs.Add(orderLine.Id);
          }
          // Set standard values on OrderLines
          orderLine.AllowOverridePrices = true;
          orderLine.Type = Convert.ToString(Convert.ToInt32(OrderLineType.Fixed));

          var multiplier = orderCreated ? 1 : orderLine.Product?.GetUnitPriceMultiplier() ?? 1.0; // if the order is created in NAV, the price the comes back already has the multiplier in it

          dAux = ReadDouble(orderLineNode, "column [@columnName='OrderLineQuantity']");
          if (dAux.HasValue)
          {
            orderLine.Quantity = dAux.Value;
          }

          // order line unit price
          PriceInfo unitPrice = new PriceInfo();

          dAux = ReadDouble(orderLineNode, "column [@columnName='OrderLineUnitPrice']");
          if (!dAux.HasValue)
          {
            dAux = ReadDouble(orderLineNode, "column [@columnName='OrderLineUnitPriceWithVAT']");
          }

          if (dAux.HasValue)
          {
            unitPrice.PriceWithVAT = dAux.Value * multiplier;
            dAux = ReadDouble(orderLineNode, "column [@columnName='OrderLineUnitPriceWithoutVAT']");
            if (dAux.HasValue)
            {
                unitPrice.PriceWithoutVAT = dAux.Value * multiplier;
            }
            else
            {
                unitPrice.PriceWithoutVAT = unitPrice.PriceWithVAT - unitPrice.VAT;
            }
          }
          else
          {
            dAux = ReadDouble(orderLineNode, "column [@columnName='OrderLineUnitPriceWithoutVAT']");
            if (dAux.HasValue)
            {
              unitPrice.PriceWithoutVAT = dAux.Value * multiplier;
              unitPrice.PriceWithVAT = unitPrice.PriceWithoutVAT + unitPrice.VAT;
            }
          }

          orderLine.SetUnitPrice(unitPrice, false);
          
          // order line price
          dAux = ReadDouble(orderLineNode, "column [@columnName='OrderLinePrice']");
          if (!dAux.HasValue)
          {
            dAux = ReadDouble(orderLineNode, "column [@columnName='OrderLinePriceWithVAT']");
          }

          if (dAux.HasValue)
          {
            // order line price with vat
            orderLine.Price.PriceWithVAT = dAux.Value * multiplier;
            dAux = ReadDouble(orderLineNode, "column [@columnName='OrderLinePriceWithoutVAT']");
            if (dAux.HasValue)
            {
                orderLine.Price.PriceWithoutVAT = dAux.Value * multiplier;
            }
            else
            {
                orderLine.Price.PriceWithoutVAT = orderLine.Price.PriceWithVAT - orderLine.Price.VAT;
            }
          }
          else
          {
            // check if order line has price without vat
            dAux = ReadDouble(orderLineNode, "column [@columnName='OrderLinePriceWithoutVAT']");
            if (dAux.HasValue)
            {
              orderLine.Price.PriceWithoutVAT = dAux.Value * multiplier;
              orderLine.Price.PriceWithVAT = orderLine.Price.PriceWithoutVAT + orderLine.Price.VAT;
            }
          }

          //orderLine.Type = Convert.ToString(Convert.ToInt32(OrderLineType.Product));

          //Set OrderLineCustomFields values
          if (AddOrderLineFieldsToRequest && orderLine.OrderLineFieldValues.Count > 0)
          {
            foreach (OrderLineFieldValue olfv in orderLine.OrderLineFieldValues)
            {
              var orderLineFieldNode = orderLineNode.SelectSingleNode(string.Format("column [@columnName='{0}']", olfv.OrderLineFieldSystemName));
              if (orderLineFieldNode != null)
              {
                olfv.Value = orderLineFieldNode.InnerText;
              }
            }
          }
        }
      }
      catch (Exception anyError)
      {
        Logger.Instance.Log(ErrorLevel.Error, string.Format("Error processing order line. Error: '{0}' productNumber = {1}.", anyError.Message, productNumber));
        throw;
      }
    }

    private static void ProcessDiscountOrderLines(Order order, OrderLineCollection discountOrderLines, XmlNode orderLineNode, string orderLineType, bool orderCreated)
    {
        Logger.Instance.Log(ErrorLevel.DebugInfo, "ProcessDiscountOrderLines..."); // TODO: removeee

        string orderLineId = orderLineNode.SelectSingleNode("column [@columnName='OrderLineId']")?.InnerText;
        string orderLineParentId = orderLineNode.SelectSingleNode("column [@columnName='OrderLineParentLineID']")?.InnerText;

      try
      {
        //parent id
        //discount name
        //discount quantity
        //discount "price" with vat
        // discount "price without vat
        double? dAux;
        var orderLine = new OrderLine
        {
          Order = order,
          OrderId = order.Id,
          Modified = DateTime.Now,
          DiscountId = (string.IsNullOrEmpty(orderLineId) ? orderLineParentId : orderLineId),
          Quantity = 1,
          Type = orderLineType,
          ProductName =  Settings.Instance.TextForDiscounts
        };

        double multiplier = 1;

        if (orderLineType == "3") //1=order discount, 3=Product Discount
        {
                //string parentProductId = orderLineNode.SelectSingleNode("column [@columnName='OrderLineProductNumber']").InnerText;
                //foreach (var prod  uctOrderLine in order.OrderLines)
                //{
                //  if (productOrderLine.ProductId == parentProductId)
                //  {
                //    orderLine.ParentLineId = productOrderLine.Id;
                //  }
                //}
                string parentProductNumber = orderLineNode.SelectSingleNode("column [@columnName='OrderLineProductNumber']").InnerText;
                foreach (var productOrderLine in order.OrderLines)
                {
                    if (string.Equals(productOrderLine.ProductNumber, parentProductNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        orderLine.ParentLineId = productOrderLine.Id;
                        //orderLine.ProductId = productOrderLine.ProductId;
                        //orderLine.ProductVariantId = productOrderLine.ProductVariantId;
                        //orderLine.ProductNumber = productOrderLine.ProductNumber;

                        // if the order is created in NAV, the price the comes back already has the multiplier in it
                        if (!orderCreated)
                        {
                            multiplier = productOrderLine.Product?.GetUnitPriceMultiplier() ?? 1;
                        }
                    }
                }
        }

        dAux = ReadDouble(orderLineNode, "column [@columnName='OrderLineQuantity']");
        if (dAux.HasValue) 
        {
          orderLine.Quantity = dAux.Value;
        }

        double priceWithoutVAT = ReadDouble(orderLineNode, "column [@columnName='OrderLinePriceWithoutVAT']").Value * multiplier;
        double priceWithVAT = ReadDouble(orderLineNode, "column [@columnName='OrderLinePriceWithVAT']").Value * multiplier;

        orderLine.UnitPrice.PriceWithoutVAT = -Math.Abs(priceWithoutVAT / Math.Max(1, orderLine.Quantity));
        orderLine.UnitPrice.PriceWithVAT = -Math.Abs(priceWithVAT / Math.Max(1, orderLine.Quantity));

        orderLine.Price.PriceWithoutVAT = -Math.Abs(priceWithoutVAT);
        orderLine.Price.PriceWithVAT = -Math.Abs(priceWithVAT);

        //orderLine.Price.PriceWithVAT =
        //    -Helpers.ParseDouble(
        //        orderLineNode.SelectSingleNode("column [@columnName='OrderLineUnitPriceWithVAT']").
        //            InnerText);

        discountOrderLines.Add(orderLine);
      }
      catch (Exception anyError)
      {
        Logger.Instance.Log(ErrorLevel.Error, string.Format("Error processing order line. Error: '{0}' OrderLineId = {1}.", anyError, orderLineId));
        throw;
      }
    }

    private static void OrderPriceCalculation(Order order, XmlNode orderNode)
    {
      var orderPriceNode = orderNode.SelectSingleNode("column [@columnName='OrderPrice']");
      var orderPriceWithoutVatNode = orderNode.SelectSingleNode("column [@columnName='OrderPriceWithoutVat']");
      var orderVat = orderNode.SelectSingleNode("column [@columnName='OrderPriceVat']");

      if (orderPriceNode == null)
      {
        orderPriceNode = orderNode.SelectSingleNode("column [@columnName='OrderPriceWithVat']");
      }

      if (orderPriceNode != null)
      {
        order.Price.PriceWithVAT = Helpers.ReadDouble(orderPriceNode.InnerText);
        GetOrderVat(order, orderVat);
        order.Price.PriceWithoutVAT = order.Price.PriceWithVAT - order.Price.VAT;
      }
      else if (orderPriceWithoutVatNode != null)
      {
        order.Price.PriceWithoutVAT = Helpers.ReadDouble(orderPriceWithoutVatNode.InnerText);
        GetOrderVat(order, orderVat);
        order.Price.PriceWithVAT = order.Price.PriceWithoutVAT + order.Price.VAT;
      }
      else
      {
        return;
      }

      order.PriceBeforeFees.PriceWithVAT = order.Price.PriceWithVAT;
      order.PriceBeforeFees.PriceWithoutVAT = order.Price.PriceWithVAT;
    }

    private static void RemoveDiscounts(Order order)
    {
      foreach (var orderLine in order.OrderLines)
      {
        if (orderLine.IsDiscount())
        {
          order.OrderLines.Remove(orderLine);
          orderLine.Delete();
        }
      }
    }

    private static OrderLine CreateOrderLine(Order order, string productNumber)
    {
      //var orderId = order == null ? "is null" : order.Id ?? "ID is null";

      //var scalar = Database.ExecuteScalar(string.Format("SELECT TOP 1 ProductID FROM EcomProducts WHERE ProductNumber = '{0}'", productNumber));
      //if (scalar == null || string.IsNullOrWhiteSpace(scalar.ToString()))
      //{
      //  Logger.Instance.Log(ErrorLevel.Error, string.Format("Cannot CreateOrderLine: No product found with ProductNumber = '{0}' Order = {1}", productNumber, orderId));
      //  return null;
      //}
      //var product = Dynamicweb.Ecommerce.Products.Product.GetProductById(scalar.ToString());
      //if (product == null)
      //{
      //      Logger.Instance.Log(ErrorLevel.Error, $"The product with id {scalar} could not be extracted. Order = {order.Id}");
      //      return null;
      //}
      //if (product.Number != productNumber)
      //{
      //  Logger.Instance.Log(ErrorLevel.Error,
      //    string.Format("Cannot CreateOrderLine: ProductNumber is different from the response ProductNumber = '{0}' Order = {1}", productNumber, orderId));
      //  return null;
      //}

      var product = Dynamicweb.Ecommerce.Products.Product.GetProductByNumber(productNumber);
      if (product == null)
      {
          Logger.Instance.Log(ErrorLevel.Error, $"The product with number {productNumber} could not be found. Order = {order.Id}");
      }

      OrderLine orderLine = new OrderLine();
      orderLine.SetProductInformation(product);
      orderLine.Type = ((int)OrderLineType.Product).ToString();
      orderLine.Order = order;
      order.OrderLines.Add(orderLine);
      return orderLine;
    }

    private static void GetOrderVat(Order order, XmlNode orderVat)
    {
      if (orderVat != null)
      {
        order.Price.VAT = Helpers.ReadDouble(orderVat.InnerText);
      }
      else
      {
        order.Price.VAT = 0;
        if (!Converter.ToBoolean(SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/Price/PricesInDbIncludesVAT")))
        {
          // not vat is used
          return;
        }
        order.Price.VAT = new PriceCalculated().GetCountryVAT();
      }
    }

    #region Session Hash helper methods

    private static string GetSessionCacheKey()
    {
      var user = User.GetCurrentExtranetUser();
      string sessionCache = Constants.CacheConfiguration.OrderCommunicationHash;

      if (user != null)
      {
        sessionCache += user.Name;
      }
      else
      {
        sessionCache += Settings.Instance.AnonymousUserKey;
      }
      return sessionCache;
    }

    private static string GetLastOrderHash()
    {
      string hashValue = null;
      object orderHash = HttpContext.Current.Session[GetSessionCacheKey()];
      if (orderHash is string)
      {
        hashValue = orderHash as string;
      }
      return hashValue;
    }

    private static void SaveOrderHash(string hashValue)
    {
      HttpContext.Current.Session[GetSessionCacheKey()] = hashValue;
    }

    private static string CalculateHash(string orderRequest)
    {
      var hashEngine = new System.Security.Cryptography.HMACMD5(new byte[] { 14, 4, 78 });
      byte[] hash = hashEngine.ComputeHash(Encoding.UTF8.GetBytes(orderRequest));
      string hashString = string.Empty;
      foreach (byte x in hash)
      {
        hashString += string.Format("{0:x2}", x);
      }
      return hashString;
    }

    #endregion
  }
}