using Kong.Portal.CLI.Services;

namespace CLI.UnitTests.Services;

internal class NullConsoleOutput : IConsoleOutput
{
    public void WriteLine(string value) { }
}
