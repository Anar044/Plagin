using Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Abstracts;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons;

public class PluginToServerHighRiskOperation : APluginToServer<PluginToServerHighRiskOperationTerminalsGroup,
    PluginToServerHighRiskOperationTerminalsGroupRestaurantSection>
{
    [JsonProperty("terminalsGroups")]
    public sealed override List<PluginToServerHighRiskOperationTerminalsGroup> TerminalsGroups { get; set; }

    public PluginToServerHighRiskOperation()
    {
        TerminalsGroups =
            base.GenerateTerminalsGroups<PluginToServerHighRiskOperationTerminalsGroup,
                PluginToServerHighRiskOperationTerminalsGroupRestaurantSection>();
    }

    /// <summary>
    /// Старый wire-формат: только группы с официантами/операциями, без пустых скелетов.
    /// </summary>
    public void RemoveEmptyTerminalsGroups()
    {
        if (TerminalsGroups == null)
            return;

        TerminalsGroups = TerminalsGroups
            .FindAll(g => g?.Waiters != null && g.Waiters.Count > 0);
    }
}

public class PluginToServerHighRiskOperationTerminalsGroup : APluginToServerTerminalsGroup<
    PluginToServerHighRiskOperationTerminalsGroupRestaurantSection>
{
    /// <summary>
    /// В старом формате highRisk секции зала не отправлялись — только waiters.
    /// </summary>
    [JsonIgnore]
    public sealed override List<PluginToServerHighRiskOperationTerminalsGroupRestaurantSection> RestaurantSections
    {
        get;
        set;
    }

    public bool ShouldSerializeRestaurantSections() => false;

    // Order < 0: waiters раньше terminalsGroupId/Name/totalTables (как в hc_250305).
    [JsonProperty("waiters", Order = -10)]
    public List<PluginToServerHighRiskOperationTerminalsGroupWaiter> Waiters { get; set; } =
        new List<PluginToServerHighRiskOperationTerminalsGroupWaiter>();
}

public class PluginToServerHighRiskOperationTerminalsGroupWaiter
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("operations")]
    public List<PluginToServerHighRiskOperationTerminalsGroupWaiterOperation> Operations { get; set; } =
        new List<PluginToServerHighRiskOperationTerminalsGroupWaiterOperation>();
}

public class PluginToServerHighRiskOperationTerminalsGroupWaiterOperation
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("lastActionDate")]
    public DateTime? LastActionDate { get; set; }
}

public class
    PluginToServerHighRiskOperationTerminalsGroupRestaurantSection : APluginToServerTerminalsGroupRestaurantSection
{
}
