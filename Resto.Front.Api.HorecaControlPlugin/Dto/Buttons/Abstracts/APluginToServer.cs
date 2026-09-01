using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin.Dto.Buttons.Abstracts;

/// <summary>
/// ���������� ��� �������� ������ ������������ ����� � ��������
/// </summary>
/// <typeparam name="TG"></typeparam>
/// <typeparam name="RS"></typeparam>
public abstract class APluginToServer<TG, RS> : APluginToServer
    where RS : APluginToServerTerminalsGroupRestaurantSection
{
    // Must be public for JSON serialization of report aggregates.
    [JsonProperty("totalTables")]
    public int TotalTables { get; set; }

    [JsonProperty("terminalsGroups")] public virtual List<TG> TerminalsGroups { get; set; }


    protected List<T1> GenerateTerminalsGroups<T1, T2>(bool onlyOne = false)
        where T1 : APluginToServerTerminalsGroup<T2>, new()
        where T2 : APluginToServerTerminalsGroupRestaurantSection, new()
    {
        var exitTerminalsGroups = PluginContext.Operations.GetRestaurantSections()
            .Where(x => x.Tables != null && x.Tables.Any(y => y.IsActive));
        if (onlyOne)
            exitTerminalsGroups = exitTerminalsGroups
                .Where(x => x.TerminalsGroup.Id == PluginHelpers.GroupName.Id).ToList();
        var terminalsGroups = new List<T1>();
        foreach (var rss in exitTerminalsGroups.GroupBy(x => x.TerminalsGroup))
        {
            var terminalGroup = rss.Key;
            var restuarantSections = new List<T2>();

            foreach (var rs in rss)
            {
                if (rs.Tables is null || !rs.Tables.Any())
                    continue;
                var restuarantSection = new T2
                {
                    Id = rs.Id,
                    Name = rs.Name,
                    TotalTables = rs.Tables.Count,
                };
                restuarantSections.Add(restuarantSection);
            }

            var totalTables = restuarantSections?.Sum(x => x.TotalTables) ?? 0;
            if (restuarantSections.Any() && totalTables > 0)
            {
                var terminal = new T1
                {
                    Id = terminalGroup.Id,
                    Name = terminalGroup.Name,
                    TotalTables = totalTables,
                    RestaurantSections = restuarantSections,
                };
                terminalsGroups.Add(terminal);
            }
        }


        return terminalsGroups;
    }
}