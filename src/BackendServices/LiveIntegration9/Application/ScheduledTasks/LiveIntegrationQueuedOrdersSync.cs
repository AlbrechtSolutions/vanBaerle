using System;
using System.Collections;
using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dynamicweb.Content.Files.Information;
using Dynamicweb.DataIntegration;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Ecommerce.Shops;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Extensibility.Editors;
using Dynamicweb.SystemTools;

namespace Dna.Ecommerce.LiveIntegration.ScheduledTasks
{
  [AddInName("LiveIntegrationQueuedOrdersSync")]
  [AddInLabel("Sync queued orders using Live Integration")]
  [AddInDescription("Sync queued orders using Live Integration")]
  [AddInIgnore(false)]
  [AddInUseParameterGrouping(true)]
  public class LiveIntegrationQueuedOrdersSync : BatchIntegrationScheduledTaskAddin, IDropDownOptions
  {
    public LiveIntegrationQueuedOrdersSync()
    {
      MinutesCompleted = 5;
      MaxOrdersToProcess = 25;
      ShopId = "";
      ExcludeRecurrent = true;
    }

    #region Parameters

    [AddInParameter("Finished for X minutes")]
    [AddInParameterEditor(typeof(IntegerNumberParameterEditor), "NewUIcheckbox=true;Value=true;")]
    [AddInParameterGroup("A) Queued Orders")]
    public int MinutesCompleted { get; set; }

    [AddInParameter("Maximum orders to process in each execution")]
    [AddInParameterEditor(typeof(IntegerNumberParameterEditor), "NewUIcheckbox=true;Value=true;")]
    [AddInParameterGroup("A) Queued Orders")]
    public int MaxOrdersToProcess { get; set; }

    [AddInParameter("Shop")]
    [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false")]
    [AddInParameterGroup("A) Queued Orders")]
    public string ShopId { get; set; }


    [AddInParameter("Order States")]
    [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false;multiple=true")]
    [AddInParameterGroup("A) Queued Orders")]
    public string OrderStates { get; set; }

    [AddInParameter("Exclude recurring order templates")]
    [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;Value=true;")]
    [AddInParameterGroup("A) Queued Orders")]
    public bool ExcludeRecurrent { get; set; }
    #endregion

    /// <summary>
    /// Main method in ScheduledTask Addin - is run when scheduled Task is run
    /// </summary>
    /// <returns></returns>
    public override bool Run()
    {
      SetupLogging();

      bool result = false;
      string error = string.Empty;

      try
      {
        string sql = string.Format(
          @"SELECT top {2} * 
          FROM EcomOrders 
          WHERE 
	          OrderComplete = 1 
	          AND OrderDeleted = 0 
	          AND (OrderIntegrationOrderID IS NULL OR OrderIntegrationOrderID = '')
	          And Isnull (OrderIsExported, 0) = 0
	          And OrderShopID = {0}
	          And IsNull (OrderIsRecurringOrderTemplate, 0) = {3}
	          AND OrderCompletedDate < DATEADD(MINUTE, -{1}, GETDATE())
	          and OrderStateID {4};"
          , string.IsNullOrEmpty(ShopId) ? "OrderShopID" : "'" + ShopId + "'", MinutesCompleted, MaxOrdersToProcess
          // recurrent order filter
          , ExcludeRecurrent ? "0" : "IsNull (OrderIsRecurringOrderTemplate, 0)"
          // order states filter
          , string.IsNullOrEmpty(OrderStates) ? " = OrderStateID" : "in ('" + OrderStates.Replace(",", "','") + "')");
        OrderCollection ordersToSync = Order.GetOrders(sql, true);

        foreach (var order in ordersToSync)
        {
          if (Global.IntegrationEnabledFor(order.ShopId))
          {
            OrderHandler.UpdateOrder(order, LiveIntegrationSubmitType.ScheduledTask);
          }
        }
        result = true;
      }
      catch (Exception e)
      {
        error = e.Message;
      }
      finally
      {
        if (error != "")
        {
          //Sendmail with error
          SendMail(error, MessageType.Error);
        }
        else
        {
          //Send mail with success
          SendMail(Translate.Translate("Scheduled task completed successfully"));
        }
      }

      WriteTaskResultToLog(result);

      return result;
    }

    public Hashtable GetOptions(string name)
    {
      var options = new Hashtable();
      switch (name)
      {
        case "Shop":
          options.Add("", "Any");
          var shops = Shop.GetShops();
          foreach (var shop in shops)
          {
            options.Add(shop.Id, shop.Name);
          }
          break;
        case "Order States":
          foreach (OrderState state in OrderState.GetAllOrderStates())
          {
            options.Add(state.Id, state.Name);
          }
          break;
      }
      return options;
    }
  }
}