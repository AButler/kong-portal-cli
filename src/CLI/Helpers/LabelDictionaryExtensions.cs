namespace Kong.Portal.CLI;

internal static class LabelDictionaryExtensions
{
    public static LabelDictionary ToLabelDictionary(this IDictionary<string, string>? labels)
    {
        return labels == null ? new LabelDictionary() : new LabelDictionary(labels);
    }
}
