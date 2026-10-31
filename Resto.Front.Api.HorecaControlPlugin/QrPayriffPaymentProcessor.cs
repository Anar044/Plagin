using System;
using Resto.Front.Api;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Organization;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.Exceptions;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.HorecaControlPlugin
{
    /// <summary>
    /// External tender processor for already PAID Payriff QR orders.
    /// No Payriff credentials are stored on the POS; payments and refunds are
    /// verified via the tenant-scoped Smart Horeca HTTPS Bridge.
    /// </summary>
    internal sealed class QrPayriffPaymentProcessor : IPaymentProcessor
    {
        private readonly QrPosBridgeService bridge;
        internal QrPayriffPaymentProcessor(QrPosBridgeService service)
        {
            bridge = service ?? throw new ArgumentNullException(nameof(service));
        }

        public string PaymentSystemKey => "smart-horeca-qr-payriff";
        public string PaymentSystemName => "Payriff (Smart Horeca QR)";

        private static PaymentActionFailedException Denied()
        {
            return new PaymentActionFailedException(
                "Payriff QR payments are made by the guest online. Do not charge on the POS.");
        }

        public void CollectData(Guid orderId, Guid paymentTypeId, IUser cashier,
            IReceiptPrinter printer, IViewManager viewManager, IPaymentDataContext context)
        {
            // Auto-created external tender has no card data to collect.
            if (!bridge.IsAlreadyPaidQrOrder(orderId))
                throw Denied();
        }

        public void OnPaymentAdded(IOrder order, IPaymentItem paymentItem,
            IUser cashier, IOperationService operations, IReceiptPrinter printer,
            IViewManager viewManager, IPaymentDataContext context)
        {
            if (order != null && !bridge.IsAlreadyPaidQrOrder(order.Id))
                throw Denied();
        }

        public bool OnPreliminaryPaymentEditing(IOrder order, IPaymentItem paymentItem,
            IUser cashier, IOperationService operations, IReceiptPrinter printer,
            IViewManager viewManager, IPaymentDataContext context) => false;

        public void OnPaymentDeleting(IOrder order, IPaymentItem paymentItem,
            IUser cashier, IReceiptPrinter printer, IViewManager viewManager,
            IPaymentDataContext context)
        {
            // Removing a processed Payriff tender must go through the POS return
            // workflow, not a silent edit of the payment item.
            throw new PaymentActionFailedException(
                "Use Return Payment in iikoFront for a Payriff QR order.");
        }

        public void Pay(decimal sum, IOrder order, IPaymentItem paymentItem,
            Guid transactionId, IPointOfSale pointOfSale, IUser cashier,
            IOperationService operations, IReceiptPrinter printer,
            IViewManager viewManager, IPaymentDataContext context)
        {
            // The money was already captured by Payriff and recorded with
            // AddExternalPaymentItem(..., isProcessed:true).
            if (order == null || !bridge.IsAlreadyPaidQrOrder(order.Id))
                throw Denied();
        }

        public bool CanPaySilently(decimal sum, Guid? orderId, Guid paymentTypeId,
            IPaymentDataContext context)
        {
            return orderId.HasValue && bridge.IsAlreadyPaidQrOrder(orderId.Value);
        }

        public void PaySilently(decimal sum, IOrder order, IPaymentItem paymentItem,
            Guid transactionId, IPointOfSale pointOfSale, IUser cashier,
            IReceiptPrinter printer, IPaymentDataContext context)
        {
            if (order == null || !bridge.IsAlreadyPaidQrOrder(order.Id))
                throw Denied();
        }

        public void ReturnPayment(decimal sum, Guid? orderId, Guid paymentTypeId,
            Guid transactionId, IPointOfSale pointOfSale, IUser cashier,
            IReceiptPrinter printer, IViewManager viewManager,
            IPaymentDataContext context)
        {
            // iiko gives the EXACT return amount and original transaction ID.
            // A failed or UNKNOWN response aborts the POS return and cannot
            // generate a second Payriff POST on retry.
            if (!orderId.HasValue || !bridge.IsAlreadyPaidQrOrder(orderId.Value))
                throw Denied();
            bridge.ReturnAlreadyPaidOrder(orderId.Value, transactionId, sum);
        }

        public void EmergencyCancelPayment(decimal sum, Guid? orderId,
            Guid paymentTypeId, Guid transactionId, IPointOfSale pointOfSale,
            IUser cashier, IReceiptPrinter printer, IViewManager viewManager,
            IPaymentDataContext context)
        {
            ReturnPayment(sum, orderId, paymentTypeId, transactionId,
                pointOfSale, cashier, printer, viewManager, context);
        }

        public void EmergencyCancelPaymentSilently(decimal sum, Guid? orderId,
            Guid paymentTypeId, Guid transactionId, IPointOfSale pointOfSale,
            IUser cashier, IReceiptPrinter printer, IPaymentDataContext context)
        {
            ReturnPayment(sum, orderId, paymentTypeId, transactionId,
                pointOfSale, cashier, printer, null, context);
        }

        public void ReturnPaymentWithoutOrder(decimal sum, Guid? orderId,
            Guid paymentTypeId, IPointOfSale pointOfSale, IUser cashier,
            IReceiptPrinter printer, IViewManager viewManager)
        {
            throw new PaymentActionFailedException(
                "Payriff QR refund requires the original iikoFront order.");
        }
    }
}
