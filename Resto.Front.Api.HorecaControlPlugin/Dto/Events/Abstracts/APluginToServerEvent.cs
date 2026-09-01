using Resto.Front.Api.HorecaControlPlugin.Dto.Events.Interfaces;
using System;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Events.Abstracts
{
    /// <summary>
    /// ����������� ��������� ��� ��������� �������
    /// </summary>
    public abstract class APluginToServerEvent : IPluginToServerEvent
    {
        [JsonProperty("updateTime")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        
        [JsonProperty("departmentName")]
        public string DepartmentName { get; set; } = PluginHelpers.DepartmentName?.Name ?? string.Empty;

        [JsonProperty("groupName")]
        public string GroupName { get; set; } = PluginHelpers.GroupName?.Name ?? string.Empty;
    }
}