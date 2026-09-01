using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons
{
    public class PluginToServerCurrentShiftOrders : APluginToServer<PluginToServerCurrentShiftOrdersTerminalsGroup,
        PluginToServerCurrentShiftOrdersTerminalsGroupRestaurantSection>
    {
        [JsonProperty("terminalsGroups")]
        public sealed override List<PluginToServerCurrentShiftOrdersTerminalsGroup> TerminalsGroups { get; set; }

        public PluginToServerCurrentShiftOrders()
        {
            TerminalsGroups =
                base.GenerateTerminalsGroups<PluginToServerCurrentShiftOrdersTerminalsGroup,
                    PluginToServerCurrentShiftOrdersTerminalsGroupRestaurantSection>(true);
            TotalTables = TerminalsGroups?.Sum(x => x.TotalTables) ?? 0;
        }
    }

    public class PluginToServerCurrentShiftOrdersTerminalsGroup : APluginToServerTerminalsGroup<
        PluginToServerCurrentShiftOrdersTerminalsGroupRestaurantSection>
    {
        [JsonProperty("restaurantSections")]
        public sealed override List<PluginToServerCurrentShiftOrdersTerminalsGroupRestaurantSection> RestaurantSections
        {
            get;
            set;
        }
    }

    public class
        PluginToServerCurrentShiftOrdersTerminalsGroupRestaurantSection : APluginToServerTerminalsGroupRestaurantSection
    {
        /// <summary>
        /// ������ ������� �������
        /// </summary>
        [JsonProperty("orders")]
        public List<CurrentShiftOrdersDto> Orders { get; set; } = new();

        /// <summary>
        /// ������ ������� �� ��������
        /// </summary>
        [JsonProperty("deliveries")]
        public List<CurrentShiftOrdersDto> Deliveries { get; set; } = new();

        /// <summary>
        /// ������ ������� ��������/��������
        /// </summary>
        [JsonProperty("reserves")]
        public List<CurrentShiftReserveDto> Reserves { get; set; } = new();
    }


    public class CurrentShiftReserveDto
    {
        /// <summary>
        /// ������ ������ � �������
        /// </summary>
        [JsonProperty("reserveTables")]
        public string ReserveTables { get; set; }

        /// <summary>
        /// ����� �������
        /// </summary>
        [JsonProperty("reserveTime")]
        public DateTime ReserveTime { get; set; }

        /// <summary>
        /// ������ �������
        /// </summary>
        [JsonProperty("reserveStatus")]
        public EnumReserveStatusDto? ReserveStatus { get; set; }

        /// <summary>
        /// ����� � �������
        /// </summary>
        [JsonProperty("reserveOrder")]
        public CurrentShiftOrdersDto ReserveOrder { get; set; }

        /// <summary>
        /// ��� ������� � �������
        /// </summary>
        [JsonProperty("reserveClientName")]
        public string ReserveClientName { get; set; }

        /// <summary>
        /// ������� ������� � �������
        /// </summary>
        [JsonProperty("reserveClientPhone")]
        public string ReserveClientPhone { get; set; }

        /// <summary>
        /// ��������� ����� ������ �������
        /// </summary>
        [JsonProperty("reserveEstimatedStartTime")]
        public DateTime ReserveEstimatedStartTime { get; set; }

        /// <summary>
        /// ����� ������ �������
        /// </summary>
        [JsonProperty("reserveStartTime")]
        public DateTime? ReserveStartTime { get; set; }

        /// <summary>
        /// ������������ �������
        /// </summary>
        [JsonProperty("reserveDuration")]
        public TimeSpan? ReserveDuration { get; set; }
    }


    public class CurrentShiftOrdersDto
    {
        /// <summary>
        /// ����� ������
        /// </summary>
        [JsonProperty("orderNum")]
        public int? OrderNum { get; set; }

        /// <summary>
        /// ���� �������� ������
        /// </summary>
        [JsonProperty("orderOpenDate")]
        public DateTime? OrderOpenDate { get; set; }

        /// <summary>
        /// ���� ������� ������
        /// </summary>
        [JsonProperty("orderBillTime")]
        public DateTime? OrderBillTime { get; set; }

        /// <summary>
        /// ���� �������� ������
        /// </summary>
        [JsonProperty("orderCloseTime")]
        public DateTime? OrderCloseTime { get; set; }

        /// <summary>
        /// ����� ������
        /// </summary>
        [JsonProperty("orderExpectedRevenue")]
        public decimal OrderExpectedRevenue { get; set; }

        /// <summary>
        /// ������ ������
        /// </summary>
        [JsonProperty("orderStatus")]
        public EnumOrderStatusDto? OrderStatus { get; set; }

        /// <summary>
        /// ������ ������ � ������
        /// </summary>
        [JsonProperty("orderTables")]
        public string OrderTables { get; set; }

        /// <summary>
        /// ������ ������ �� ��������
        /// </summary>
        [JsonProperty("deliveryOrderStatus")]
        public EnumDeliveryOrderStatusDto? DeliveryOrderStatus { get; set; }

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
        public DateTime DeliveryCreateTime { get; set; }

        /// <summary>
        /// Time when products were printed
        /// </summary>
        [JsonProperty("deliveryPrintTime")]
        public DateTime? DeliveryPrintTime { get; set; }

        /// <summary>
        /// Delivery open time
        /// </summary>
        [JsonProperty("deliveryOpenTime")]
        public DateTime DeliveryOpenTime { get; set; }

        /// <summary>
        /// Time when order was sent to client
        /// </summary>
        [JsonProperty("deliverySendTime")]
        public DateTime? DeliverySendTime { get; set; }

        /// <summary>
        /// Time when client wants the order to be delivered
        /// </summary>
        [JsonProperty("deliveryExpectedDeliverTime")]
        public DateTime DeliveryExpectedDeliverTime { get; set; }

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

        /// <summary>
        /// ��� ������������ ������ ��������/���������
        /// </summary>
        [JsonProperty("deliveryServiceType")]
        public string DeliveryServiceType { get; set; }
    }
}