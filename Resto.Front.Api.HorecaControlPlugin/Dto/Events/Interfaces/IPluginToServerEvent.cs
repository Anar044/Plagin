using System;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events.Interfaces;

/// <summary>
/// Интерфейс плагинного события
/// </summary>
public interface IPluginToServerEvent
{
    /// <summary>
    /// Время обновления события 
    /// </summary>
    [JsonProperty("updateTime")]
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// Название департамента
    /// </summary>
    [JsonProperty("departmentName")]
    public string DepartmentName { get; set; }

    /// <summary>
    /// Название терминальной группы
    /// </summary>
    [JsonProperty("groupName")]
    public string GroupName { get; set; }
}