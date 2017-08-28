using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dynamicweb.Controls;
using Dynamicweb.Controls.Extensibility;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Extensibility.AddIns;

namespace Dna.Ecommerce.LiveIntegration.Addin.RibbonBar
{
  [AddInName("Resend order")]
  [AddInDescription("Resend order from backend")]
  [AddInTarget(RibbonBarAddInTarget.eCom.OrderEdit)]
  [AddInTarget(RibbonBarAddInTarget.eCom.OrderList)]
  public class ResendOrderAddIn : RibbonBarAddInBase
  {
    private string SessionKey
    {
      get
      {
        var key = "LiveIntegration.TransferredOrder";
        var order = Ribbon.DataContext.DataSource as Order;
        if (order != null)
        {
          key += order.Id;
        }
        return key;
      }
    }

    public ResendOrderAddIn(Dynamicweb.Controls.RibbonBar ribbon) : base(ribbon) { }

    public override void Load()
    {
      var button = CreateTransferOrderButton(base.CreateLiveIntegrationRibbon("Orders"));
      button.EnableServerClick = true;

      //If Order list is loaded
      var source = Ribbon.DataContext.DataSource as Order[];
      if (source != null)
      {
        var orders = source;
        if (orders.Any(o => o != null && string.IsNullOrEmpty(o.IntegrationOrderId)))
        {
          button.OnClientClick = "if(!confirm('Transfer selected orders to ERP?')) { return false; };";
          button.Click += TransferOrdersButton_Click;
        }
        else
        {
          button.OnClientClick = "alert('All selected orders were already transferred');";
        }
      }
      else
      {
        //If Order edit is loaded
        if (Ribbon.DataContext.DataSource is Order)
        {
          // Access the DataContext which is an Order for this AddIn.
          var order = (Order)Ribbon.DataContext.DataSource;
          if (string.IsNullOrEmpty(order.IntegrationOrderId))
          {
            button.OnClientClick = string.Format("if(!confirm('Transfer order {0} to ERP?')) {{ return false; }};", order.Id);
            button.Click += TransferOrderButton_Click;
          }
          else
          {
            button.OnClientClick = string.Format(
              "if(!confirm('Order {0} is already in the ERP, update again?')) {{ return false; }};", order.Id);
            button.Click += TransferOrderButton_Click;
          }
        }
      }
    }

    public override void Render(HtmlTextWriter writer)
    {
      if (HttpContext.Current.Session[SessionKey] != null && !string.IsNullOrEmpty(HttpContext.Current.Session[SessionKey].ToString()))
      {
        writer.Write("<script>alert('{0}')</script>", HttpUtility.HtmlEncode(HttpContext.Current.Session[SessionKey]));
        HttpContext.Current.Session.Remove(SessionKey);
      }

      base.Render(writer);
    }

    /// <summary>
    /// Occurs when the button was clicked from edit order page
    /// </summary>        
    private void TransferOrderButton_Click(object sender, EventArgs e)
    {
      string output = string.Empty;

      var order = Ribbon.DataContext.DataSource as Order;
      if (order != null)
      {
        //if (string.IsNullOrEmpty(order.IntegrationOrderId))
        //{
        if (TransferOrder(order))
        {
          output = "Order successfully transferred to the ERP.";
        }
        else
        {
          output = "Error creating order in the ERP. See LiveIntegration log for details.";
        }
        //}
        //else
        //{
        //  output = "Order already transferred.";
        //}
      }
      HttpContext.Current.Session[SessionKey] = output;
    }

    /// <summary>
    /// Occurs when the button was clicked from orders list page
    /// </summary>        
    private void TransferOrdersButton_Click(object sender, EventArgs e)
    {
      string output = string.Empty;

      if (HttpContext.Current.Session["DW.Controls.Tree.List"] != null && HttpContext.Current.Session["DW.Controls.Tree.List"] is ListRowCollection)
      {
        //Get selected orders from order list
        var rows = HttpContext.Current.Session["DW.Controls.Tree.List"] as ListRowCollection;
        if (rows != null && rows.Count > 0 && rows.Any(r => r.Selected))
        {
          List<string> exportedOrders = new List<string>();
          List<string> alreadyExportedOrders = new List<string>();

          foreach (string id in rows.Where(r => r.Selected).Select(r => r.ItemID).Distinct())
          {
            Order o = Order.GetOrderById(id);
            if (o != null)
            {
              if (string.IsNullOrEmpty(o.IntegrationOrderId))
              {
                if (TransferOrder(o))
                {
                  exportedOrders.Add(o.Id);
                }
              }
              else
              {
                alreadyExportedOrders.Add(o.Id);
              }
            }
          }
          if (alreadyExportedOrders.Count > 0 && alreadyExportedOrders.Count == rows.Count(r => r.Selected))
          {
            output = "All selected orders are already transferred to ERP.";
          }
          else if (exportedOrders.Count > 0 || alreadyExportedOrders.Count > 0)
          {
            if ((exportedOrders.Count + alreadyExportedOrders.Count) == rows.Count(r => r.Selected))
            {
              output = "All selected orders were successfully transferred to ERP.";
            }
            else
            {
              if (alreadyExportedOrders.Count > 0)
              {
                output += string.Format("Orders with IDs [{0}] were already transferred to ERP. ", string.Join(",", alreadyExportedOrders));
              }
              if (exportedOrders.Count > 0)
              {
                output += string.Format("Orders with IDs [{0}] were successfully transferred to ERP. ", string.Join(",", exportedOrders));
              }
              output += string.Format("Orders with IDs [{0}] were not transferred to ERP. See LiveIntegration log for details", string.Join(",", rows.Where(r => r.Selected && !(exportedOrders.Contains(r.ItemID) || alreadyExportedOrders.Contains(r.ItemID))).Select(r => r.ItemID).Distinct().ToArray()));
            }
          }
          else
          {
            output = "None of the selected orders were transferred to ERP. See LiveIntegration log for details.";
          }
        }
        else
        {
          output = "No orders selected for transfer";
        }
      }
      else
      {
        output = "Can not load selected orders from the order list";
      }
      HttpContext.Current.Session[SessionKey] = output;
    }

    private bool TransferOrder(Order order)
    {
      bool? result = OrderHandler.UpdateOrder(order, LiveIntegrationSubmitType.ManualSubmit);
      return result.HasValue && result.Value;
    }
  }
}