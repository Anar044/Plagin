using System;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Interfaces;

/// <summary>
/// Интерфес для отправки ответов на сервер
/// </summary>
public interface IPluginToServer
{
    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    [JsonProperty("updateTime")]
    public DateTime UpdateTime { get; set; }

    public bool ShouldSerializeUpdateTime();

    /// <summary>
    /// Название департамента
    /// </summary>
    [JsonProperty("departmentName")]
    public string DepartmentName { get; set; }

    public bool ShouldSerializeDepartmentName();

    public bool ShouldSerializeHeader { get; set; }
}