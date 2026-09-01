using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Interfaces;
using System;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

public abstract class APluginToServer : IPluginToServer
{
    [JsonProperty("updateTime")]
    public DateTime UpdateTime { get; set; } = DateTime.Now;
    public bool ShouldSerializeUpdateTime() => ShouldSerializeHeader;
    
    [JsonProperty("departmentName")]
    public string DepartmentName { get; set; } = PluginHelpers.DepartmentName.Name;
    public bool ShouldSerializeDepartmentName() => ShouldSerializeHeader;
    
    [JsonIgnore] public bool ShouldSerializeHeader { get; set; } = true;
}