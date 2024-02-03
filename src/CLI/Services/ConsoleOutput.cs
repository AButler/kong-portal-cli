namespace Kong.Portal.CLI.Services;

public class ConsoleOutput : IConsoleOutput
{
    public void WriteLine(string value)
    {
        Console.WriteLine(value);
    }
}
