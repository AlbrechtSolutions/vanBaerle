using System;
using System.Collections;
using System.Linq;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dynamicweb.Content.Files.Information;
using Dynamicweb.DataIntegration;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Extensibility.Editors;
using Dynamicweb.SystemTools;

namespace Dna.Ecommerce.LiveIntegration.ScheduledTasks
{
  [AddInName("LiveIntegrationUpdateOrders")]
  [AddInLabel("Update orders based on custom fields")]
  [AddInDescription("Update orders based on custom fields to update order state and this will generate emails notifications.")]
  [AddInIgnore(false)]
  [AddInUseParameterGrouping(true)]
  public class LiveIntegrationUpdateOrders : BatchIntegrationScheduledTaskAddin, IDropDownOptions
	{
		private string _processError;

		public LiveIntegrationUpdateOrders()
		{
			// default values
			MaxOrdersToProcess = 25;
			MinutesCompleted = 5;
			UpdateCustomField = true;
			NewState = "";
			FilterState = "";
			ForceStateUpdate = false;
		}

		#region Hide base properties/methods
		private new bool LogRequestAndResponse { get; set; }
		private new string NotificationEmail { get; set; }
		private new bool NotificationEmailFailureOnly { get; set; }
		private new string NotificationEmailSenderEmail { get; set; }
		private new string NotificationEmailSenderName { get; set; }
		private new string NotificationEmailSubject { get; set; }
		private new string NotificationTemplate { get; set; }
		#endregion

		#region Parameters
		#region A) General

	  [AddInParameter("Finished for X minutes")]
	  [AddInParameterEditor(typeof(IntegerNumberParameterEditor), "allowNegativeValues=false;NewGUI=true;")]
	  [AddInParameterGroup("A) General Filter")]
	  public int MinutesCompleted { get; set; }

	  [AddInParameter("Maximum orders per execution")]
	  [AddInParameterEditor(typeof(IntegerNumberParameterEditor), "allowNegativeValues=false;NewGUI=true;")]
	  [AddInParameterGroup("A) General Filter")]
	  public int MaxOrdersToProcess { get; set; }

	  [AddInParameter("State Filter")]
	  [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;multiple=true;SortBy=Key")]
	  [AddInParameterGroup("A) General Filter")]
	  public string FilterState { get; set; }
		#endregion

		#region B) Fields to compare

	  [AddInParameter("Field A")]
	  [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;multiple=false;SortBy=Key")]
	  [AddInParameterGroup("B) Fields to compare")]
	  public string FieldA { get; set; }

	  [AddInParameter("Field B")]
	  [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;multiple=false;SortBy=Key")]
	  [AddInParameterGroup("B) Fields to compare")]
	  public string FieldB { get; set; }
		#endregion

		#region C) Action on update

	  [AddInParameter("Update custom fields (Field B replaces Field A)")]
	  [AddInParameterEditor(typeof(YesNoParameterEditor), "NewGUI=false")]
	  [AddInParameterGroup("C) Action on update")]
	  public bool UpdateCustomField { get; set; }

	  [AddInParameter("Order State")]
	  [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false;multiple=false;SortBy=Key")]
	  [AddInParameterGroup("C) Action on update")]
	  public string NewState { get; set; }

	  [AddInParameter("Force State update")]
	  [AddInParameterEditor(typeof(YesNoParameterEditor), "NewGUI=false")]
	  [AddInParameterGroup("C) Action on update")]
	  public bool ForceStateUpdate { get; set; }
		#endregion

		#region D) Notification

	  [AddInParameter("Subject")]
	  [AddInParameterEditor(typeof(TextParameterEditor), "NewGUI=true;")]
	  [AddInParameterGroup("D) Notification")]
	  public string Subject { get; set; }

	  [AddInParameter("Sender Name")]
	  [AddInParameterEditor(typeof(TextParameterEditor), "NewGUI=true;")]
	  [AddInParameterGroup("D) Notification")]
	  public string SenderName { get; set; }

	  [AddInParameter("Sender e-mail")]
	  [AddInParameterEditor(typeof(TextParameterEditor), "NewGUI=true;")]
	  [AddInParameterGroup("D) Notification")]
	  public string SenderEmail { get; set; }

	  [AddInParameter("Send to")]
	  [AddInParameterEditor(typeof(TextParameterEditor), "NewGUI=true;")]
	  [AddInParameterGroup("D) Notification")]
	  public string SendTo { get; set; }

	  [AddInParameter("Send email to customer")]
	  [AddInParameterEditor(typeof(YesNoParameterEditor), "NewGUI=false;")]
	  [AddInParameterGroup("D) Notification")]
	  public bool SendToCustomer { get; set; }

	  [AddInParameter("Email template")]
	  [AddInParameterEditor(typeof(TemplateParameterEditor), "NewGUI=true;folder=eCom7/CartV2/Mail")]
	  [AddInParameterGroup("D) Notification")]
	  public string EmailTemplate { get; set; }
		#endregion
		#endregion

		/// <summary>
		/// Main method in ScheduledTask Addin - is run when scheduled Task is run
		/// </summary>
		/// <returns></returns>
		public override bool Run()
		{
			SetupLogging();

			bool processResult = true;
			_processError = "";
			try
			{
				// validate parameters
				if (string.IsNullOrEmpty(FieldA))
				{
				  throw new ArgumentException("Invalid field parameter!", "Field A");
				}

			  if (string.IsNullOrEmpty(FieldB))
			  {
			    throw new ArgumentException("Invalid field parameter!", "Field B");
			  }

			  // get the relevant data from the DB
        var ordersToSync = ReadOrders();

				if (processResult && ordersToSync != null)
				{
				  ProcessOrders(ordersToSync);
				}
				else
				{
				  Logger.Log("No orders to be processed.");
				}
			}
			catch (Exception ex)
			{
				_processError = ex.Message;
				Logger.Log(string.Format("Error processing Update Orders job {0}\n {1}", ex.Message, ex));
			}
			finally
			{
				// handled errors during process.
				if (_processError != "")
				{
					// Sendmail with error
					processResult = false;
					SendMail(_processError, MessageType.Error);
				}
				else
				{
					// Send mail with success 
					SendMail(Translate.Translate("Scheduled task completed successfully"));
				}
			}

			WriteTaskResultToLog(processResult);
			return processResult;
		}

		private OrderCollection ReadOrders()
		{
			System.Text.StringBuilder sbSql = new System.Text.StringBuilder();

			sbSql.AppendFormat(
          @"SELECT top {0} * 
          FROM EcomOrders 
          WHERE OrderComplete = 1 AND OrderDeleted = 0 
	          and OrderCompletedDate < DATEADD(MINUTE, {1}, GETDATE())
	          and {2} != {3}"
				,  MaxOrdersToProcess, MinutesCompleted, FieldA, FieldB);

			if (!string.IsNullOrEmpty(FilterState))
			{
			  sbSql.AppendFormat("\n	AND OrderStateID in ('{0}')", FilterState.Replace(",", "','"));
			}

		  return Order.GetOrders(sbSql.ToString(), true);
		}

		private void ProcessOrders(OrderCollection ordersToSync)
		{
			Logger.Log(string.Format("The list of order to run: {0}", string.Join(",",  ordersToSync)));

			foreach (Order order in ordersToSync)
			{
				string currentOrderId = order.Id;
				try
				{
					// update custom field (if wanted)
					if (UpdateCustomField)
					{
						var fieldA = order.OrderFieldValues.First(of => of.OrderField.SystemName == FieldA);
						var fieldB = order.OrderFieldValues.First(of => of.OrderField.SystemName == FieldB);

						fieldA.Value = fieldB.Value;
					}

					// update order state
					if (!string.IsNullOrWhiteSpace(NewState))
					{
						// check if it's needed to force the change state
						if(order.StateId == NewState && ForceStateUpdate)
						{
						  order.StateId = "";
						}

					  order.StateId = NewState;
					}

					// save changes to fire notifications
					order.Save();

					// send new notification from batch parameters
					SendNotification(order);

					Logger.Log(string.Format("Order ID {0} done!", currentOrderId));
				}
				catch (Exception ex)
				{
					_processError += string.Format("Error in Update. Order ID = {0}, Exception = {1}", currentOrderId, ex.ToString());
					Logger.Log(string.Format("Error in Update. Order ID = {0}, Exception = {1}", currentOrderId, ex.ToString()));
				}
				finally
				{
					// save capture changes and try to update ERP
					if (Configuration.Global.IntegrationEnabledFor(order.ShopId))
					{
						bool? result = OrderHandler.UpdateOrder(order, LiveIntegrationSubmitType.ScheduledTask);
						if (result.HasValue && !result.Value)
						{
						  _processError += string.Format("Order ID {0} Successfully updated but not updated to the ERP.\n"
						    , currentOrderId);
						}
					}
				}
			}

			Logger.Log("All orders done!");
		}

		private void SendNotification(Order order)
		{
			// confirm that settings are valid to send
			if (string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(SenderEmail) || string.IsNullOrEmpty(SendTo) && !SendToCustomer)
			{
			  return;
			}

		  if (string.IsNullOrWhiteSpace(SenderName))
		  {
		    SenderName = null;
		  }

		  // prepare data to send
			var slTo = new System.Collections.Generic.List<string>();
			var page = Dynamicweb.Frontend.PageView.GetPageviewByPageID(order.CheckoutPageId);
			var template = new Dynamicweb.Rendering.Template(EmailTemplate.Replace("Templates/", ""));

			if (!string.IsNullOrEmpty(SendTo))
			{
			  slTo.AddRange(SendTo.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
			}
		  if (SendToCustomer)
		  {
		    slTo.Add(order.CustomerEmail);
		  }

		  order.SendTo(ref page, Subject, slTo, SenderEmail, SenderName, ref template);
		}

		/// <summary>
		/// Gets options for the various drop down lists in this addin.
		/// </summary>
		/// <param name="name">The label of the control for which the options should be returned.</param>
		/// <returns></returns>
		public Hashtable GetOptions(string name)
		{
			var options = new Hashtable();
			switch (name)
			{
				case "Field A":
				case "Field B":
					var order = new Order();
					foreach (var field in order.OrderFieldValues)
					{
					  options.Add(field.OrderField.SystemName, field.OrderField.Name);
					}
					break;
				case "Order State":
					options.Add("", "Leave unchanged");
					goto case "State Filter";
				case "State Filter":
					foreach (var state in OrderState.GetAllOrderStates())
					{
					  options.Add(state.Id, state.Name);
					}
			    break;
			}
			return options;
		}
	}
}