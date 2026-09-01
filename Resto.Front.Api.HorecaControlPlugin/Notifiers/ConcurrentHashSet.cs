using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Resto.Front.Api.HorecaControlPlugin.Notifiers;

public class ConcurrentHashSet<T>
{
    private readonly ConcurrentDictionary<T, bool> _dict = new ConcurrentDictionary<T, bool>();

    public bool Add(T item) => _dict.TryAdd(item, true);
    public bool Remove(T item) => _dict.TryRemove(item, out _);
    public bool Contains(T item) => _dict.ContainsKey(item);
    public void Clear() => _dict.Clear();
    public int Count => _dict.Count;
}

public static class ConcurrentHashSetExtensions
{
    public static ConcurrentHashSet<T> ToConcurrentHashSet<T>(this IEnumerable<T> source)
    {
        var set = new ConcurrentHashSet<T>();
        foreach (var item in source)
        {
            set.Add(item);
        }

        return set;
    }

    public static ConcurrentBag<T> ToConcurrentBag<T>(this IEnumerable<T> source)
    {
        if (source == null)
            return new ConcurrentBag<T>();
        return new ConcurrentBag<T>(source);
    }
}