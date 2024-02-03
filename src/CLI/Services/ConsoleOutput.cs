namespace Kong.Portal.CLI.Services;

internal class ConsoleOutput : IConsoleOutput
{
    public void WriteLine(string value)
    {
        Console.WriteLine(value);
    }
}
