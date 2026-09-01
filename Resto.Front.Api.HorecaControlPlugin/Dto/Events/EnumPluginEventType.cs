using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events;

[JsonConverter(typeof(StringEnumConverter))]
public enum EnumPluginEventType
{
    //   | 'closeCashRegisterShift'
    [EnumMember(Value = "closeCashRegisterShift")]
    CloseCashRegisterShift,

    //   | 'openCashRegisterShift'
    [EnumMember(Value = "openCashRegisterShift")]
    OpenCashRegisterShift,

    //   | 'cashRegisterStart'
    [EnumMember(Value = "cashRegisterStart")]
    CashRegisterStart,

    //   | 'cashRegisterShutDown'
    [EnumMember(Value = "cashRegisterShutDown")]
    CashRegisterShutDown,

    //   | 'newOrder'
    [EnumMember(Value = "newOrder")] NewOrder,

    //   | 'deletionOfNotPrintedItem'
    [EnumMember(Value = "deletionOfNotPrintedItem")]
    DeletionOfNotPrintedItem,

    //   | 'deletionOfPrintedItem'
    [EnumMember(Value = "deletionOfPrintedItem")]
    DeletionOfPrintedItem,

    //   | 'addingDiscount'
    [EnumMember(Value = "addingDiscount")] AddingDiscount,

    //   | 'removingDiscount'
    [EnumMember(Value = "removingDiscount")]
    RemovingDiscount,

    //   | 'orderGuestBill'
    [EnumMember(Value = "orderGuestBill")] OrderGuestBill,

    //   | 'cancellationOfGuestBill'
    [EnumMember(Value = "cancellationOfGuestBill")]
    CancellationOfGuestBill,

    //   | 'voidReceipt'
    [EnumMember(Value = "voidReceipt")] VoidReceipt,

    //   | 'deletingAnOrder'
    [EnumMember(Value = "deletingAnOrder")]
    DeletingAnOrder,

    //   | 'closingOrder'
    [EnumMember(Value = "closingOrder")] ClosingOrder,

    //   | 'orderTableHasBeenChanged'
    [EnumMember(Value = "orderTableHasBeenChanged")]
    OrderTableHasBeenChanged,

    //   | 'ordersWaiterHasChanged'
    [EnumMember(Value = "ordersWaiterHasChanged")]
    OrdersWaiterHasChanged,

    //   | 'severalOrderShifts'
    [EnumMember(Value = "severalOrderShifts")]
    SeveralOrderShifts,

    //   | 'orderAmountHasReachedMaximum'
    [EnumMember(Value = "orderAmountHasReachedMaximum")]
    OrderAmountHasReachedMaximum,

    //   | 'aLotOfOrdersWereOpenedInARow'
    [EnumMember(Value = "aLotOfOrdersWereOpenedInARow")]
    ALotOfOrdersWereOpenedInARow,

    //   | 'thePeriodBetweenGuestBillAndCheckIsTooLong'
    [EnumMember(Value = "thePeriodBetweenGuestBillAndCheckIsTooLong")]
    ThePeriodBetweenGuestBillAndCheckIsTooLong,

    //   | 'deliveryTimeEditing'
    [EnumMember(Value = "deliveryTimeEditing")]
    DeliveryTimeEditing,

    //   | 'deliveryCancellationApplyingDiscountAndPayment'
    [EnumMember(Value = "deliveryCancellationApplyingDiscountAndPayment")]
    DeliveryCancellationApplyingDiscountAndPayment,

    //   | 'cancellationForClosedDelivery'
    [EnumMember(Value = "cancellationForClosedDelivery")]
    CancellationForClosedDelivery,

    //   | 'changeItemsAmountOnStopList'
    [EnumMember(Value = "changeItemsAmountOnStopList")]
    ChangeItemsAmountOnStopList,

    //   | 'removeFromStopList'
    [EnumMember(Value = "removeFromStopList")]
    RemoveFromStopList,

    //   | 'orderInTheStatusOfTheBillForALongTime'
    [EnumMember(Value = "orderInTheStatusOfTheBillForALongTime")]
    OrderInTheStatusOfTheBillForALongTime,

    //   | 'deleteAnEmptyOrder'
    [EnumMember(Value = "deleteAnEmptyOrder")]
    DeleteAnEmptyOrder,

    //   | 'deliveryOrderCookedWaitingForDispatch'
    [EnumMember(Value = "deliveryOrderCookedWaitingForDispatch")]
    DeliveryOrderCookedWaitingForDispatch,

    //   | 'deliveryOrderOnItsWay'
    [EnumMember(Value = "deliveryOrderOnItsWay")]
    DeliveryOrderOnItsWay,


    //   | 'deliveryOrderDelivered'
    [EnumMember(Value = "deliveryOrderDelivered")]
    DeliveryOrderDelivered,

    //   | 'deliveryOrderNotDelivered'
    [EnumMember(Value = "deliveryOrderNotDelivered")]
    DeliveryOrderNotDelivered,

    //   | 'deliveryOrderCancelled'
    [EnumMember(Value = "deliveryOrderCancelled")]
    DeliveryOrderCancelled,

    //   | 'banquetOrderCancelled'
    [EnumMember(Value = "banquetOrderCancelled")]
    BanquetOrderCancelled,

    //   | 'addingSurcharge'
    [EnumMember(Value = "addingSurcharge")]
    AddingSurcharge,

    //   | 'removingSurcharge'
    [EnumMember(Value = "removingSurcharge")]
    RemovingSurcharge,

    //   | 'reserveIsCancelled'
    [EnumMember(Value = "reserveIsCancelled")]
    ReserveIsCancelled,

    //   | 'employeesPersonalShiftOpen'
    [EnumMember(Value = "employeesPersonalShiftOpen")]
    EmployeesPersonalShiftOpen,

    //   | 'employeesPersonalShiftClosed'
    [EnumMember(Value = "employeesPersonalShiftClosed")]
    EmployeesPersonalShiftClosed,

    //   | 'printer'
    [EnumMember(Value = "printer")] Printer,

    //   | 'multipleCustomerRegistrationInOrders'
    [EnumMember(Value = "multipleCustomerRegistrationInOrders")]
    MultipleCustomerRegistrationInOrders,

    //   | 'newBanquetOrReservation';
    [EnumMember(Value = "newBanquetOrReservation")]
    NewBanquetOrReservation,
}