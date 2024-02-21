namespace Kong.Portal.CLI;

internal class OutputErrorException : Exception
{
    public OutputErrorException(string message)
        : base(message) { }

    public OutputErrorException(string message, Exception innerException)
        : base(message, innerException) { }
}
