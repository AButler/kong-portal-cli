namespace Kong.Portal.CLI;

internal class TokenNotFoundException : OutputErrorException
{
    public TokenNotFoundException()
        : this("Option '--konnect-token' or '--konnect-token-file' is required.") { }

    public TokenNotFoundException(string message)
        : base(message) { }

    public TokenNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
