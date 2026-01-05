namespace Kong.Portal.CLI;

internal static class LabelDictionaryExtensions
{
    extension(IDictionary<string, string>? labels)
    {
        public LabelDictionary ToLabelDictionary()
        {
            return labels == null ? new LabelDictionary() : new LabelDictionary(labels);
        }
    }
}
