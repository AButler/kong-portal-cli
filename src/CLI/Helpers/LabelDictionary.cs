namespace Kong.Portal.CLI;

internal class LabelDictionary : Dictionary<string, string>, IEquatable<LabelDictionary>
{
    public LabelDictionary() { }

    public LabelDictionary(IDictionary<string, string> dictionary)
        : base(dictionary) { }

    public bool Equals(LabelDictionary? other)
    {
        if (other == null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!Keys.ToHashSet().SetEquals(other.Keys.ToHashSet()))
        {
            return false;
        }

        return this.All(kvp => StringComparer.InvariantCulture.Equals(kvp.Value, other[kvp.Key]));
    }

    public LabelDictionary Clone()
    {
        return new LabelDictionary(this.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }
}
