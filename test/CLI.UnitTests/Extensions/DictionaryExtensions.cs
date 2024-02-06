namespace CLI.UnitTests;

internal static class DictionaryExtensions
{
    public static IDictionary<string, string?> ToNullableValueDictionary(this IDictionary<string, string> original)
    {
        var dictionary = new Dictionary<string, string?>();

        foreach (var kvp in original)
        {
            dictionary.Add(kvp.Key, kvp.Value);
        }

        return dictionary;
    }
}
