using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

public class PluginToServerOrderDetails : APluginToServer
{
    [JsonProperty("orderType")]
    public string OrderType { get; set; }


    /// <summary>
    /// �������� ������ ����������
    /// </summary>
    [JsonProperty("terminalsGroupName")]
    public string TerminalsGroupNameName { get; set; } = PluginHelpers.GroupName.Name;

    /// <summary>
    /// ������ ������
    /// </summary>
    [JsonProperty("tables")]
    public string Tables { get; set; }

    /// <summary>
    /// ����� ������
    /// </summary>
    [JsonProperty("orderNum")]
    public int OrderNum { get; set; }

    /// <summary>
    /// ����� ������
    /// </summary>
    [JsonProperty("floor")]
    public string Floor { get; set; }


    /// <summary>
    /// ID ���������
    /// </summary>
    [JsonProperty("waiterId")]
    public string WaiterId { get; set; }

    /// <summary>
    /// ��������
    /// </summary>
    [JsonProperty("waiter")]
    public string Waiter { get; set; }


    /// <summary>
    /// ������
    /// </summary>
    [JsonProperty("cashierId")]
    public string CashierId { get; set; }

    /// <summary>
    /// ������
    /// </summary>
    [JsonProperty("cashier")]
    public string Cashier { get; set; }

    /// <summary>
    /// ����� �������� ������
    /// </summary>
    [JsonProperty("openTime")]
    public DateTime? OpenTime { get; set; }

    /// <summary>
    /// ����� �������� ������
    /// </summary>
    [JsonProperty("closeTime")]
    public DateTime? CloseTime { get; set; }

    /// <summary>
    /// ����� ������� ������
    /// </summary>
    [JsonProperty("billTime")]
    public DateTime? BillTime { get; set; }

    /// <summary>
    /// ����� ������
    /// </summary>
    [JsonProperty("revenue")]
    public decimal? Revenue { get; set; }

    /// <summary>
    /// ��������� �����
    /// </summary>

    [JsonProperty("isBanquet")]
    public bool IsBanquet { get; set; }

    /// <summary>
    /// ������ ������
    /// </summary>
    [JsonProperty("orderStatus")]
    public EnumOrderStatusDto? OrderStatus { get; set; }

    /// <summary>
    /// ������ ��������
    /// </summary>
    [JsonProperty("deliveryStatus")]
    public EnumDeliveryOrderStatusDto? DeliveryStatus { get; set; }

    /// <summary>
    /// ������ ������
    /// </summary>
    [JsonProperty("discounts")]
    public List<KeyValueClass> Discounts { get; set; }

    /// <summary>
    /// ������ ��������
    /// </summary>
    [JsonProperty("surcharges")]
    public List<KeyValueClass> Surcharges { get; set; }

    /// <summary>
    /// ������ �����
    /// </summary>
    [JsonProperty("payments")]
    public List<KeyValueClass> Payments { get; set; }

    /// <summary>
    /// ������ ������
    /// </summary>

    [JsonProperty("tips")]
    public List<KeyValueClass> Tips { get; set; }

    /// <summary>
    /// ������ �������
    /// </summary>
    [JsonProperty("items")]
    public List<PluginToServerOrderDetailsItem> Items { get; set; }

    /// <summary>
    /// ����������� �����
    /// </summary>
    [JsonProperty("isDelivery")]
    public bool IsDelivery { get; set; }

    /// <summary>
    /// ��� ��������
    /// </summary>
    [JsonProperty("deliveryServiceType")]
    public string DeliveryServiceType { get; set; }

    /// <summary>
    /// ���������� ������
    /// </summary>
    [JsonProperty("guestCount")]
    public int GuestCount { get; set; }

    /// <summary>
    /// ��� ������� ��������
    /// </summary>
    [JsonProperty("deliveryClient")]
    public string DeliveryClient { get; set; }

    /// <summary>
    /// ������� ������� ��������
    /// </summary>
    [JsonProperty("deliveryPhone")]
    public string DeliveryPhone { get; set; }


    /// <summary>
    /// ����� �������� �������
    /// </summary>
    [JsonProperty("reserveClientPhone")]
    public string ReserveClientPhone { get; set; }

    /// <summary>
    /// ��� ������� �������
    /// </summary>
    [JsonProperty("reserveClientName")]
    public string ReserveClientName { get; set; }

    /// <summary>
    /// �������� ����� ������ �������
    /// </summary>
    [JsonProperty("reserveGuestComingTime")]
    public DateTime? ReserveGuestComingTime { get; set; }

    /// <summary>
    /// ��������� ����� �������
    /// </summary>
    [JsonProperty("reserveDuration")]
    public TimeSpan? ReserveDuration { get; set; }

    /// <summary>
    /// ��������� ���� ������� ������ �������
    /// </summary>
    [JsonProperty("reserveEstimatedStartTime")]
    public DateTime? ReserveEstimatedStartTime { get; set; }


    /// <summary>
    /// ����� ��������
    /// </summary>
    [JsonProperty("deliveryAddress")]
    public string DeliveryAddress { get; set; }

    /// <summary>
    /// Time when the order was marked as cancelled
    /// </summary>
    [JsonProperty("deliveryCancelTime")]
    public DateTime? DeliveryCancelTime { get; set; }

    /// <summary>
    /// Time when order was confirmed
    /// </summary>
    [JsonProperty("deliveryConfirmTime")]
    public DateTime? DeliveryConfirmTime { get; set; }

    /// <summary>
    /// Time when order was created
    /// </summary>
    [JsonProperty("deliveryCreateTime")]
    public DateTime? DeliveryCreateTime { get; set; }

    /// <summary>
    /// Time when products were printed
    /// </summary>
    [JsonProperty("deliveryPrintTime")]
    public DateTime? DeliveryPrintTime { get; set; }

    /// <summary>
    /// Delivery open time
    /// </summary>
    [JsonProperty("deliveryOpenTime")]
    public DateTime? DeliveryOpenTime { get; set; }

    /// <summary>
    /// Time when order was sent to client
    /// </summary>
    [JsonProperty("deliverySendTime")]
    public DateTime? DeliverySendTime { get; set; }

    /// <summary>
    /// Time when client wants the order to be delivered
    /// </summary>
    [JsonProperty("deliveryExpectedDeliverTime")]
    public DateTime? DeliveryExpectedDeliverTime { get; set; }

    /// <summary>
    /// Time when order cooking was finished
    /// </summary>
    [JsonProperty("deliveryCookingFinishTime")]
    public DateTime? DeliveryCookingFinishTime { get; set; }

    /// <summary>
    /// Time when delivery order was marked as closed
    /// </summary>
    [JsonProperty("deliveryDeliveryCloseTime")]
    public DateTime? DeliveryDeliveryCloseTime { get; set; }

    /// <summary>
    /// Predicted time when order cooking should be completed
    /// </summary>
    [JsonProperty("deliveryPredictedCookingCompleteTime")]
    public DateTime? DeliveryPredictedCookingCompleteTime { get; set; }

    /// <summary>
    /// Time when order was actually delivered to the client
    /// </summary>
    [JsonProperty("deliveryActualDeliverTime")]
    public DateTime? DeliveryActualDeliverTime { get; set; }

    /// <summary>
    /// Predicted time when order should be delivered
    /// </summary>
    [JsonProperty("deliveryPredictedDeliveryTime")]
    public DateTime? DeliveryPredictedDeliveryTime { get; set; }

    /// <summary>
    /// Duration of delivery
    /// </summary>
    [JsonProperty("deliveryDuration")]
    public TimeSpan? DeliveryDuration { get; set; }

    /// <summary>
    /// Duration calculated without overridden value on terminal
    /// </summary>
    [JsonProperty("deliveryExpectedDuration")]
    public TimeSpan? DeliveryExpectedDuration { get; set; }
}

/// <summary>
/// ������ �������
/// </summary>
public class PluginToServerOrderDetailsItem
{
    /// <summary>
    /// ������ �������������
    /// </summary>
    [JsonProperty("modifiers")]
    public List<PluginToServerOrderDetailsItemModifier> Modifiers { get; set; }

    /// <summary>
    /// ����� ������ �������
    /// </summary>
    [JsonProperty("printTime")]
    public DateTime? PrintTime { get; set; }

    /// <summary>
    /// ������ �������
    /// </summary>
    [JsonProperty("status")]
    public EnumOrderItemStatusDto Status { get; set; }

    /// <summary>
    /// �������� �������
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// ����� ������������� �������
    /// </summary>
    [JsonProperty("cookingTime")]
    public TimeSpan? CookingTime { get; set; }

    /// <summary>
    /// ����� ������ �������
    /// </summary>
    [JsonProperty("serveTime")]
    public DateTime? ServeTime { get; set; }

    /// <summary>
    /// ����� ������ ������������� �������
    /// </summary>
    [JsonProperty("cookingStartTime")]
    public DateTime? CookingStartTime { get; set; }

    /// <summary>
    /// ����� ��������� ������������� �������
    /// </summary>
    [JsonProperty("cookingFinishTime")]
    public DateTime? CookingFinishTime { get; set; }

    /// <summary>
    /// ���������� �������
    /// </summary>
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// ������ �������
    /// </summary>
    [JsonProperty("size")]
    public string Size { get; set; }

    /// <summary>
    /// ������������� ����� �������
    /// </summary>
    [JsonProperty("resultSum")]
    public decimal ResultSum { get; set; }

    /// <summary>
    /// ������� ���� �������
    /// </summary>
    [JsonProperty("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// ������������� ��� ��������� ���������
    /// </summary>
    [JsonProperty("tineLimit")]
    public TimeSpan? TimeLimit { get; set; }
}

/// <summary>
/// ������ �������������
/// </summary>
public class PluginToServerOrderDetailsItemModifier
{
    /// <summary>
    /// �������� ������������
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// ���������� �������
    /// </summary>
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// ������������� ����� �������
    /// </summary>
    [JsonProperty("resultSum")]
    public decimal ResultSum { get; set; }

    /// <summary>
    /// ������� ���� �������
    /// </summary>
    [JsonProperty("price")]
    public decimal Price { get; set; }
}