using Kong.Portal.CLI;

namespace CLI.UnitTests.Services;

internal class MockEnvironmentVariableReader : IEnvironmentVariableReader
{
    private readonly IDictionary<string, string> _variables = new Dictionary<string, string>();

    public string? Get(string variable)
    {
        return _variables.TryGetValue(variable, out var value) ? value : null;
    }

    public void Add(string variable, string value)
    {
        _variables.Add(variable, value);
    }
}
