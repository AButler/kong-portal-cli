namespace Kong.Portal.CLI;

internal class SyncException : Exception
{
    public SyncException(string message)
        : base(message) { }

    public SyncException(string message, Exception innerException)
        : base(message, innerException) { }
}
