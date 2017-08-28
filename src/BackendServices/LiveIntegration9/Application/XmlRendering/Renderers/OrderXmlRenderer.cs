using System;
using System.Linq;
using System.Xml;
using Dna.Ecommerce.LiveIntegration.Addin;
using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.Extensions;
using Dna.Ecommerce.LiveIntegration.ExtensionsMethods;
using Dna.Ecommerce.LiveIntegration.XmlRendering.RenderSettings;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Environment;
using Dynamicweb.Extensibility.Notifications;
using Dynamicweb.Security.UserManagement;

namespace Dna.Ecommerce.LiveIntegration.XmlRendering.Renderers
{
    internal class OrderXmlRenderer : XmlRenderer
    {
        struct CreditCardGatewayResult
        {
            public string Number { get; set; }
            public string Type { get; set; }
            public string ExpiryDate { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        internal string RenderOrderXml(Order order, RenderOrderSettings settings)
        {
            NotificationManager.Notify(Notifications.LiveIntegration.Orders.OnBeforeRenderOrderXml, new Notifications.LiveIntegration.Orders.OnBeforeRenderOrderXmlArgs(order, settings));
            var xmlDocument = BuildXmlDocument();
            var tablesNode = CreateAndAppendTablesNode(xmlDocument, settings);
            tablesNode.AppendChild(BuildOrderXml(xmlDocument, order, settings));
            tablesNode.AppendChild(BuildOrderLinesXml(xmlDocument, order));
            if (settings.AddOrderLineFieldsToRequest)
            {
                tablesNode.AppendChild(BuildOrderLineFieldsXml(xmlDocument, order));
            }
            NotificationManager.Notify(Notifications.LiveIntegration.Orders.OnAfterRenderOrderXml, new Notifications.LiveIntegration.Orders.OnAfterRenderOrderXmlArgs(order, xmlDocument));
            if (settings.Beautify)
            {
                return xmlDocument.Beautify();
            }
            return xmlDocument.InnerXml;
        }

        private XmlNode BuildOrderXml(XmlDocument xmlDocument, Order order, RenderOrderSettings settings)
        {
            var tableNode = CreateTableNode(xmlDocument, "EcomOrders");
            var itemNode = CreateAndAppendItemNode(tableNode, "EcomOrders");

            var user = User.GetUserByID(order.CustomerAccessUserId) ?? (!ExecutingContext.IsBackEnd() ? User.GetCurrentUser() : null);

            // do not use order.Modified in XML unless the field can be ignored for hash calculation

            AddChildXmlNode(itemNode, "OrderCustomerAccessUserExternalId", !string.IsNullOrWhiteSpace(user?.ExternalID) ? user.ExternalID : Settings.Instance.AnonymousUserKey);
            AddChildXmlNode(itemNode, "OrderCustomerNumber", !string.IsNullOrWhiteSpace(user?.CustomerNumber) ? user.CustomerNumber : Settings.Instance.AnonymousUserKey);
            AddChildXmlNode(itemNode, "CreateOrder", settings.CreateOrder.ToString());
            AddChildXmlNode(itemNode, "OrderId", order.Id);
            AddChildXmlNode(itemNode, "OrderAutoId", order.AutoId.ToString());
            AddChildXmlNode(itemNode, "OrderIntegrationOrderId", order.IntegrationOrderId);
            AddChildXmlNode(itemNode, "OrderCurrencyCode", order.CurrencyCode);
            AddChildXmlNode(itemNode, "OrderDate", order.Date.ToIntegrationString());
            AddChildXmlNode(itemNode, "OrderPaymentMethodName", order.PaymentMethod, true);
            AddChildXmlNode(itemNode, "OrderPaymentMethodId", order.PaymentMethodId);
            AddChildXmlNode(itemNode, "OrderShippingMethodName", order.ShippingMethod, true);
            AddChildXmlNode(itemNode, "OrderShippingMethodId", order.ShippingMethodId);
            AddChildXmlNode(itemNode, "OrderShippingFee", order.ShippingFee.PriceWithoutVAT.ToIntegrationString());
            AddChildXmlNode(itemNode, "OrderCustomerName", order.CustomerName);
            AddChildXmlNode(itemNode, "OrderCustomerAddress", order.CustomerAddress);
            AddChildXmlNode(itemNode, "OrderCustomerAddress2", order.CustomerAddress2);
            AddChildXmlNode(itemNode, "OrderCustomerCity", order.CustomerCity);
            AddChildXmlNode(itemNode, "OrderCustomerState", order.CustomerRegion);
            AddChildXmlNode(itemNode, "OrderCustomerZip", order.CustomerZip);
            AddChildXmlNode(itemNode, "OrderCustomerCountryCode", order.CustomerCountryCode);
            AddChildXmlNode(itemNode, "OrderCustomerEmail", order.CustomerEmail);
            AddChildXmlNode(itemNode, "OrderCustomerPhone", order.CustomerPhone);
            AddChildXmlNode(itemNode, "OrderCustomerFax", order.CustomerFax);
            AddChildXmlNode(itemNode, "OrderDeliveryName", !string.IsNullOrWhiteSpace(order.DeliveryName) ? order.DeliveryName : order.CustomerName);
            AddChildXmlNode(itemNode, "OrderDeliveryAddress", order.DeliveryAddress);
            AddChildXmlNode(itemNode, "OrderDeliveryAddress2", order.DeliveryAddress2);
            AddChildXmlNode(itemNode, "OrderDeliveryCity", order.DeliveryCity);
            AddChildXmlNode(itemNode, "OrderDeliveryState", order.DeliveryRegion);
            AddChildXmlNode(itemNode, "OrderDeliveryZip", order.DeliveryZip);
            AddChildXmlNode(itemNode, "OrderDeliveryCountryCode", order.DeliveryCountryCode);
            AddChildXmlNode(itemNode, "OrderDeliveryEmail", order.DeliveryEmail);
            AddChildXmlNode(itemNode, "OrderDeliveryPhone", order.DeliveryPhone);
            AddChildXmlNode(itemNode, "OrderDeliveryFax", order.DeliveryFax);
            AddChildXmlNode(itemNode, "OrderCustomerComment", order.CustomerComment);
            AddChildXmlNode(itemNode, "OrderPriceTotal", order.TotalPrice.ToIntegrationString());
            AddChildXmlNode(itemNode, "OrderCaptureAmount", order.CaptureAmount.ToIntegrationString());
            AddChildXmlNode(itemNode, "OrderVoucherCode", order.VoucherCode);
            AddChildXmlNode(itemNode, "OrderTransactionId", order.TransactionNumber);
            AddChildXmlNode(itemNode, "OrderStateId", order.StateId);
            AddChildXmlNode(itemNode, "OrderStateName", order.OrderState.Name, true);

            CreditCardGatewayResult paymentInformation = ReadPaymentInformation(order);

            AddChildXmlNode(itemNode, "OrderTransactionCardType", paymentInformation.Type, true);
            AddChildXmlNode(itemNode, "OrderTransactionCardExpiryDate", paymentInformation.ExpiryDate, true);
            AddChildXmlNode(itemNode, "OrderTransactionMaskedCreditCardNumber", paymentInformation.Number, true);
            AddChildXmlNode(itemNode, "CardHolderFirstName", paymentInformation.FirstName, true);
            AddChildXmlNode(itemNode, "CardholderLastName", paymentInformation.LastName, true);

            if (settings.AddOrderFieldsToRequest)
            {
                AppendOrderFields(order, itemNode);
            }
            return tableNode;
        }

        private static CreditCardGatewayResult ReadPaymentInformation(Order order)
        {
            CreditCardGatewayResult paymentInformation = new CreditCardGatewayResult();
            if (!string.IsNullOrEmpty(order.GatewayResult))
            {
                var payResult = new XmlDocument();
                payResult.LoadXml(order.GatewayResult);

                var element = payResult.SelectSingleNode("//req_card_number");
                paymentInformation.Number = element?.InnerText;

                element = payResult.SelectSingleNode("//req_bill_to_surname");
                paymentInformation.LastName = element?.InnerText;

                element = payResult.SelectSingleNode("//req_ship_to_surname");
                paymentInformation.FirstName = element?.InnerText;

                element = payResult.SelectSingleNode("//req_card_expiry_date");
                paymentInformation.ExpiryDate = element?.InnerText;

                element = payResult.SelectSingleNode("//req_card_type");
                paymentInformation.Type = element?.InnerText;
            }

            return paymentInformation;
        }

        private XmlNode BuildOrderLinesXml(XmlDocument xmlDocument, Order order)
        {
            var tableNode = CreateTableNode(xmlDocument, "EcomOrderLines");

            // Order lines (products, taxes)
            foreach (var orderLine in order.OrderLines.Where(ol => !ol.IsDiscount()))
            {
                CreateOrderLineXml(tableNode, orderLine);
            }

            // Order lines (order discounts, and product discounts)
            foreach (var orderLine in order.OrderLines.Where(ol => ol.IsDiscount()))
            {
                CreateOrderLineXml(tableNode, orderLine);
            }
            return tableNode;
        }

        private void AppendOrderFields(Order order, XmlElement orderNode)
        {
            foreach (var field in order.OrderFieldValues)
            {
                string value;
                if (field.Value is double)
                {
                    value = Convert.ToDouble(field.Value).ToIntegrationString();
                }
                else if (field.Value is DateTime)
                {
                    value = Convert.ToDateTime(field.Value).ToIntegrationString();
                }
                else
                {
                    value = field.Value?.ToString() ?? string.Empty;
                }
                AddChildXmlNode(orderNode, field.OrderField.SystemName, value, isCustomField: true);
            }
        }

        private void CreateOrderLineXml(XmlNode orderLinesNode, OrderLine ol)
        {
            NotificationManager.Notify(Notifications.LiveIntegration.OrderLines.OnBeforeRenderOrderLineXml, new Notifications.LiveIntegration.OrderLines.OnBeforeRenderOrderLineXmlArgs(ol));

            if (ol.Bom && !Settings.Instance.AddOrderLinePartsToRequest)
            {
                return;
            }
            var itemNode = CreateAndAppendItemNode(orderLinesNode, "EcomOrderLines");

            // try do decode order type and put name
            OrderLineType orderLineType;
            var orderLineTypeName = Enum.TryParse(ol.Type, out orderLineType) ? orderLineType.ToString() : ol.Type;

            // do not use ol.Date in XML unless the field can be ignored for hash calculation
            // do not use ol.Modified in XML unless the field can be ignored for hash calculation
            AddChildXmlNode(itemNode, "OrderLineId", ol.Id);
            AddChildXmlNode(itemNode, "OrderLineOrderId", ol.OrderId);
            AddChildXmlNode(itemNode, "OrderLineParentLineId", ol.ParentLineId);
            AddChildXmlNode(itemNode, "OrderLineProductId", ol.ProductId);
            AddChildXmlNode(itemNode, "OrderLineProductVariantId", ol.ProductVariantId);
            AddChildXmlNode(itemNode, "OrderLineProductNumber", ol.ProductNumber);
            AddChildXmlNode(itemNode, "OrderLineProductName", ol.ProductName); // Product name or discount description
            AddChildXmlNode(itemNode, "OrderLineQuantity", ol.Quantity.ToIntegrationString());
            AddChildXmlNode(itemNode, "OrderLinePriceWithoutVat", ol.Price.PriceWithoutVAT.ToIntegrationString());
            AddChildXmlNode(itemNode, "OrderLineUnitPriceWithoutVat", ol.UnitPrice.PriceWithoutVAT.ToIntegrationString());
            AddChildXmlNode(itemNode, "NetPrice", ol.IsProduct() ? ol.Product.DefaultPrice.ToIntegrationString() : "");
            AddChildXmlNode(itemNode, "OrderLineType", ol.Type);
            AddChildXmlNode(itemNode, "OrderLineTypeName", orderLineTypeName, true);
            AddChildXmlNode(itemNode, "OrderLineBom", ol.Bom.ToString());
            AddChildXmlNode(itemNode, "OrderLineBomItemId", ol.BomItemId);
            AddChildXmlNode(itemNode, "OrderLineGiftCardCode", ol.GiftCardCode);
            AddChildXmlNode(itemNode, "OrderLineIsGiftCardDiscount", (!string.IsNullOrEmpty(ol.GiftCardCode)).ToString(), isCustomField: true);
            AddChildXmlNode(itemNode, "OrderLineFieldValues", ol.OrderLineFieldValues.ToXml().InnerXml);

            if (Settings.Instance.AddOrderLinePartsToRequest)
            {
                foreach (var kitProductOrderLine in ol.BomOrderLines)
                {
                    CreateOrderLineXml(orderLinesNode, kitProductOrderLine);
                }
            }
            NotificationManager.Notify(Notifications.LiveIntegration.OrderLines.OnAfterRenderOrderLineXml, new Notifications.LiveIntegration.OrderLines.OnAfterRenderOrderLineXmlArgs(ol, orderLinesNode));
        }

        private XmlNode BuildOrderLineFieldsXml(XmlDocument xmlDocument, Order order)
        {
            var orderLineFields = CreateTableNode(xmlDocument, "EcomOrderLineFields");
            foreach (var orderLine in order.OrderLines.Where(x => x.OrderLineFieldValues.Any()))
            {
                var itemNode = CreateAndAppendItemNode(orderLineFields, "EcomOrderLineFields");
                foreach (var field in orderLine.OrderLineFieldValues)
                {
                    AddChildXmlNode(itemNode, "OrderLineFieldOrderLineId", orderLine.Id);
                    AddChildXmlNode(itemNode, "OrderLineFieldName", field.OrderLineFieldName ?? "");
                    AddChildXmlNode(itemNode, "OrderLineFieldValue", field.Value ?? "");
                    AddChildXmlNode(itemNode, "OrderLineFieldSystemName", field.OrderLineFieldSystemName ?? "");
                }
            }
            return orderLineFields;
        }
    }
}