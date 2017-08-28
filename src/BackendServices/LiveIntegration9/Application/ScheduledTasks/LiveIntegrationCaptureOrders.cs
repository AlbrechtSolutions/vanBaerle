using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.XmlRendering;
using Dynamicweb.Content.Files.Information;
using Dynamicweb.DataIntegration;
using Dynamicweb.Ecommerce.Cart;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Ecommerce.Orders.Gateways;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Extensibility.Editors;
using Dynamicweb.SystemTools;

namespace Dna.Ecommerce.LiveIntegration.ScheduledTasks
{
  [AddInName("LiveIntegrationCaptureOrders")]
  [AddInLabel("Capture orders using Live Integration")]
  [AddInDescription("Capture orders payments updated through Batch Integration and updating using Live Integration")]
  [AddInIgnore(false)]
  [AddInUseParameterGrouping(true)]
  public class LiveIntegrationCaptureOrders : BatchIntegrationScheduledTaskAddin, IDropDownOptions
	{
		private readonly Dictionary<string, CheckoutHandler> _paymentHandlers = new Dictionary<string, CheckoutHandler>();
		private string _processError;

		#region Parameters
		#region A Type of filter
		#region Filters checked

	  [AddInParameter("Field and Value options")]
	  [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
	  [AddInParameterGroup("A) Type of filter")]
	  public bool FilterBy_FieldAndValue { get; set; }

	  [AddInParameter("Status")]
	  [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
	  [AddInParameterGroup("A) Type of filter")]
	  public bool FilterBy_Status { get; set; }

	  [AddInParameter("DoCapturePayment Boolean Flag")]
	  [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
	  [AddInParameterGroup("A) Type of filter")]
	  public bool FilterBy_doCapturePaymentFlag { get; set; } = true;

	  [AddInParameter("Single Order")]
	  [AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
	  [AddInParameterGroup("A) Type of filter")]
	  public bool FilterBy_SingleOrder { get; set; }
		#endregion

	  [AddInParameter("Field")]
	  [AddInParameterEditor(typeof(DropDownParameterEditor), "inputClass=NewUIinput;SortBy=Key;")]
	  [AddInParameterGroup("A.1) Field and Value options")]
	  public string FieldToSearch { get; set; }

	  [AddInParameter("Value")]
	  [AddInParameterEditor(typeof(TextParameterEditor), "inputClass=NewUIinput;")]
	  [AddInParameterGroup("A.1) Field and Value options")]
	  public string ValueToSearch { get; set; }

	  [AddInParameter("Order States")]
	  [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false;multiple=true;SortBy=Key")]
	  [AddInParameterGroup("A.2) Status filter")]
	  public string StatusToSearch { get; set; }

	  [AddInParameter("Order ID")]
	  [AddInParameterEditor(typeof(TextParameterEditor), "inputClass=NewUIinput;")]
	  [AddInParameterGroup("A.3) Single Order")]
	  public string OrderId { get; set; }
		#endregion

		#region B

	  [AddInParameter("Finished for X minutes")]
	  [AddInParameterEditor(typeof(IntegerNumberParameterEditor), "NewUIcheckbox=true;Value=true;")]
	  [AddInParameterGroup("B) Action")]
	  public int MinutesCompleted { get; set; } = 5;

	  [AddInParameter("Maximum orders per execution")]
	  [AddInParameterEditor(typeof(IntegerNumberParameterEditor), "NewUIcheckbox=true;Value=true;")]
	  [AddInParameterGroup("B) Action")]
	  public int MaxOrdersToProcess { get; set; } = 25;

	  #endregion

		#region C

	  [AddInParameter("If success")]
	  [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false;SortBy=Key;")]
	  [AddInDescription("Empty means leave unchanged")]
	  [AddInParameterGroup("C) Order status")]
	  public string SuccessValue { get; set; }

	  [AddInParameter("If error")]
	  [AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;none=false;SortBy=Key;")]
	  [AddInDescription("Empty means leave unchanged")]
	  [AddInParameterGroup("C) Order status")]
	  public string ErrorValue { get; set; }

		[AddInParameter("Update order to ERP after capture")]
		[AddInParameterEditor(typeof(YesNoParameterEditor), "NewUIcheckbox=true;")]
		[AddInParameterGroup("C) Order status")]
		public bool SendBackToERP { get; set; } = true;
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
				//confirm that order table has is custom flags to check payment status
				CheckOrderCustomFields();

				//get the relevant data from the DB
				var ordersToSync = ReadOrders();

				if (ordersToSync != null)
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
				Logger.Log(string.Format("Error processing Capture job {0}\n {1}", ex.Message, ex));
			}
			finally
			{
				//handled errors during process.
				if (_processError != "")
				{
					//Sendmail with error
					processResult = false;
					SendMail(_processError, MessageType.Error);
				}
				else
				{
					//Send mail with success 
					SendMail(Translate.Translate("Scheduled task completed successfully"));
				}
			}

			WriteTaskResultToLog(processResult);
			return processResult;
		}

		private void CheckOrderCustomFields()
		{
			var order = new Order();

			if (FilterBy_doCapturePaymentFlag && !order.OrderFields.Any(of => of.SystemName == "DoCapturePayment"))
			{
			  var doCapturePayment = new OrderField
			  {
			    SystemName = "DoCapturePayment",
			    Locked = true,
			    Name = "Make Capture Payment",
			    TypeId = 3,
			    TypeName = "checkbox"
			  };

			  doCapturePayment.Save("Order_DoCapturePayment");
				order.OrderFields.Add(doCapturePayment);
			}

			if (!order.OrderFields.Any(of => of.SystemName == "PaymentCaptured"))
			{
			  var paymentCaptured = new OrderField
			  {
			    SystemName = "PaymentCaptured",
			    Locked = true,
			    Name = "Payment is done",
			    TypeId = 3,
			    TypeName = "checkbox"
			  };

			  paymentCaptured.Save("Order_PaymentCaptured");
				order.OrderFields.Add(paymentCaptured);
			}
		}

		private OrderCollection ReadOrders()
		{
			//build header sql to get order
			string sql = string.Format(
              @"SELECT top {0} * 
              FROM EcomOrders 
              WHERE OrderComplete = 1 AND OrderDeleted = 0 
              And IsNull (PaymentCaptured, 0) = 0
              AND OrderCompletedDate < DATEADD(MINUTE, {1}, GETDATE())
              ",  MaxOrdersToProcess, MinutesCompleted);

			//for each optional filter add is condition to the sql
			sql += ReadOrderFromCustomFields();
			sql += ReadOrderFromStatus();
			sql += ReadOrderFromOrderId();
			sql += ReadOrderFromDoCapturePaymentFlag();

			return Order.GetOrders(sql, true);
		}

		private string ReadOrderFromCustomFields()
		{
			if (FilterBy_FieldAndValue && !string.IsNullOrEmpty(FieldToSearch))
			{
			  return string.Format("	AND {0} = '{1}'\n", FieldToSearch, ValueToSearch.Replace("'", "''"));
			}
		  return "";
		}

		private string ReadOrderFromStatus()
		{
			if (FilterBy_Status && !string.IsNullOrEmpty(StatusToSearch))
			{
			  return string.Format("	AND OrderStateID in ('{0}')\n", StatusToSearch.Replace(",", "','"));
			}
		  return "";
		}

		private string ReadOrderFromOrderId()
		{
			if (FilterBy_SingleOrder && !string.IsNullOrEmpty(OrderId))
			{
			  return string.Format("	AND OrderID = '{0}'\n", OrderId);
			}
		  return "";
		}

		private string ReadOrderFromDoCapturePaymentFlag()
		{
			if (FilterBy_doCapturePaymentFlag)
			{
				return "	 And IsNull (DoCapturePayment, 0) = 1\n";
			}

			return "";
		}


		private void ProcessOrders(OrderCollection ordersToSync)
		{
			string ordersToProcess = "";
			foreach (var order in ordersToSync)
			{
				ordersToProcess += order.Id + ",";
			}
			Logger.Log(string.Format("The list of order to run: {0}", ordersToProcess));

			foreach (var order in ordersToSync)
			{
				string state = "";
				string currentOrderId = order.Id;

				try
				{
					OrderCaptureInfo result;
					CheckoutHandler ch = CheckPaymentMethod(order);
					var payed = order.OrderFieldValues.First(of => of.OrderField.SystemName == "PaymentCaptured");

					payed.Value = false;

					if (ch != null && ch is IRemoteCapture)
					{
						result = order.Capture();
					}
					else
					{
						throw new ArgumentException("Payment method not valid for capture!", "order.PaymentMethodId");
					}

					Logger.Log(string.Format("Order ID {0} Successfuly Captured with result: {1}", currentOrderId, result != null ? result.Message : "{null}"));

					//update custom fields to avoid being payed again
					payed.Value = true;

					Logger.Log(string.Format("Order ID {0} done!", currentOrderId));

					state = SuccessValue;
				}
				catch (Exception ex)
				{
					state = ErrorValue;
					_processError += string.Format("Error in Capture. Order ID = {0}, Exception = {1}", currentOrderId, ex);
					Logger.Log(string.Format("Error in Capture. Order ID = {0}, Exception = {1}", currentOrderId, ex));
				}
				finally
				{
					if (SendBackToERP)
					{
						// save capture changes and try to update ERP
						if (!Global.IntegrationEnabledFor(order.ShopId))
						{
							UpdateOrder(order, state);
						}
						else
						{
							bool? result = OrderHandler.UpdateOrder(order, LiveIntegrationSubmitType.CaptureTask, SuccessValue, ErrorValue);
							if (result == null)
							{
								UpdateOrder(order, state);
							}
							else if (!result.Value)
							{
								_processError += string.Format("Order ID {0} successfully captured but not updated to the ERP.\n"
								  , currentOrderId);
							}
						}
					}
				}
			}

			Logger.Log("All orders done!");
		}

		private CheckoutHandler CheckPaymentMethod(Order order)
		{
			CheckoutHandler ch = null;
			var payId = order.PaymentMethodId;

			if (!string.IsNullOrEmpty(payId))
			{
				if (_paymentHandlers.ContainsKey(payId))
				{
				  ch = _paymentHandlers[payId];
				}
				else
				{
					ch = CheckoutHandler.GetCheckoutHandlerFromPaymentID(payId);
					_paymentHandlers.Add(payId, ch);
				}
			}

			return ch;
		}

		private static void UpdateOrder(Order order, string state)
		{
			if (!string.IsNullOrWhiteSpace(state))
			{
			  order.StateId = state;
			}

		  order.Save();
		}

		/// <summary>
		/// gets options for Import Activity Dropdown
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Hashtable GetOptions(string name)
		{
			var options = new Hashtable();
			switch (name)
			{
				case "Field":
					var order = new Order();
					foreach (var fieldValue in order.OrderFieldValues)
					{
					  options.Add(fieldValue.OrderField.SystemName, fieldValue.OrderField.Name);
					}
			    break;
				case "If success":
				case "If error":
					options.Add("", "Leave unchanged");
					goto case "Order States";
				case "Order States":
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