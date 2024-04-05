namespace Kong.Portal.CLI;

internal interface IEnvironmentVariableReader
{
    string? Get(string variable);
}
