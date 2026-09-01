using System;
using System.Collections.Generic;
using System.Linq;

namespace Resto.Front.Api.HorecaControlPlugin.Sql;

internal class ChangesTracker<T1, T2>
{
    private readonly IEnumerable<T1> oldValues;
    private readonly IEnumerable<T2> newValues;
    private readonly Func<T1, T2, bool, bool> areEqual1;


    public ChangesTracker(IEnumerable<T1> oldValues, IEnumerable<T2> newValues, Func<T1, T2, bool, bool> areEqual1)
    {
        this.oldValues = oldValues;
        this.newValues = newValues;
        this.areEqual1 = areEqual1;
    }

    public IEnumerable<T2> AddedItems => newValues.Where(n => oldValues.All(o => !areEqual1(o, n, false)));

    public IEnumerable<T1> RemovedItems => oldValues.Where(n => newValues.All(o => !areEqual1(n, o, false)));

    public IEnumerable<T2> UpdatedItems => newValues.Where(n => oldValues.Any(o => areEqual1(o, n, true)));

    public IEnumerable<T1> UpdatedItemsOld => oldValues.Where(n => newValues.Any(o => areEqual1(n, o, true)));
}