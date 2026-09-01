using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

/// <summary>
/// ��� ������
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum EnumPaymentType
{
    /// <summary>
    /// ��������
    /// </summary>
    [EnumMember(Value = "Cash")] Cash,

    /// <summary>
    /// ���������� �����
    /// </summary>
    [EnumMember(Value = "Card")] Card,

    /// <summary>
    /// ������
    /// </summary>
    [EnumMember(Value = "Other")] Other,
}

/// <summary>
/// ������ ������
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum EnumOrderStatusDto
{
    /// <summary>
    /// ����� �����
    /// </summary>
    [EnumMember(Value = "New")] New,

    /// <summary>
    /// ������
    /// </summary>
    [EnumMember(Value = "Bill")] Bill,

    /// <summary>
    /// �������� �����
    /// </summary>
    [EnumMember(Value = "Closed")] Closed,

    /// <summary>
    /// ���������� �����
    /// </summary>
    [EnumMember(Value = "Deleted")] Deleted,
}

/// <summary>
/// ������ ������� � ������
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum EnumOrderItemStatusDto
{
    /// <summary>Newly added item.</summary>
    [EnumMember(Value = "Added")] Added,

    /// <summary>Item was printed with low priority (<see cref="F:Resto.Front.Api.Data.Orders.OrderItemCourse.Second" />, <see cref="F:Resto.Front.Api.Data.Orders.OrderItemCourse.Third" />, <see cref="F:Resto.Front.Api.Data.Orders.OrderItemCourse.Fourth" />) and corresponding course serve cheque wasn't printed yet.</summary>
    [EnumMember(Value = "PrintedNotCooking")]
    PrintedNotCooking,

    /// <summary>Order item was printed with high or normal priority (<see cref="F:Resto.Front.Api.Data.Orders.OrderItemCourse.Vip" />, <see cref="F:Resto.Front.Api.Data.Orders.OrderItemCourse.First" />) or was printed with low priority and it's corresponding course serve cheque was also printed.</summary>
    [EnumMember(Value = "CookingStarted")] CookingStarted,

    /// <summary>Order item cooking completed, so it's ready to serve.</summary>
    [EnumMember(Value = "CookingCompleted")]
    CookingCompleted,

    /// <summary>Order item is served.</summary>
    [EnumMember(Value = "Served")] Served,
}

/// <summary>
/// ������ ��������
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum EnumDeliveryOrderStatusDto
{
    /// <summary>
    /// ���������������
    /// </summary>
    [EnumMember(Value = "Unconfirmed")] Unconfirmed,

    /// <summary>
    /// �����
    /// </summary>
    [EnumMember(Value = "New")] New,

    /// <summary>
    /// ��������
    /// </summary>
    [EnumMember(Value = "Waiting")] Waiting,

    /// <summary>
    /// � ����
    /// </summary>
    [EnumMember(Value = "OnWay")] OnWay,

    /// <summary>
    /// ���������
    /// </summary>
    [EnumMember(Value = "Delivered")] Delivered,

    /// <summary>
    /// ������
    /// </summary>
    [EnumMember(Value = "Closed")] Closed,

    /// <summary>
    /// �������
    /// </summary>
    [EnumMember(Value = "Cancelled")] Cancelled,
}

/// <summary>
/// ������ ��������������
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum EnumReserveStatusDto
{
    /// <summary>
    /// �����
    /// </summary>
    [EnumMember(Value = "New")] New,

    /// <summary>
    /// �������
    /// </summary>
    [EnumMember(Value = "Started")] Started,

    /// <summary>
    /// ��������
    /// </summary>
    [EnumMember(Value = "Closed")] Closed,
}