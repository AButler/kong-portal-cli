namespace Kong.Portal.CLI;

internal class DictionaryComparer<TKey, TValue>(IEqualityComparer<TValue> valueComparer) : IEqualityComparer<IDictionary<TKey, TValue>>
{
    public bool Equals(IDictionary<TKey, TValue>? x, IDictionary<TKey, TValue>? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        if (x.Count != y.Count)
        {
            return false;
        }

        if (!x.Keys.ToHashSet().SetEquals(y.Keys.ToHashSet()))
        {
            return false;
        }

        return x.All(kvp => valueComparer.Equals(kvp.Value, y[kvp.Key]));
    }

    public int GetHashCode(IDictionary<TKey, TValue> obj)
    {
        return obj.GetHashCode();
    }
}
