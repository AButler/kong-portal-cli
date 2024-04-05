namespace Kong.Portal.CLI;

internal class EnvironmentVariableReader : IEnvironmentVariableReader
{
    public string? Get(string variable) => Environment.GetEnvironmentVariable(variable);
}
