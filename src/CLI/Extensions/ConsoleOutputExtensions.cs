using Kong.Portal.CLI.Services;
using Kong.Portal.CLI.Services.Models;

namespace Kong.Portal.CLI;

internal static class ConsoleOutputExtensions
{
    public static void WriteDifference<T>(this IConsoleOutput consoleOutput, Difference<T> difference, string entityName, int indentLevel = 0)
    {
        var spaces = new string(' ', indentLevel * 2);

        consoleOutput.WriteLine($"{spaces}{difference.DifferenceType.ToSymbol()} {entityName}");
    }

    public static void WriteDifference<T>(
        this IConsoleOutput consoleOutput,
        Difference<T> difference,
        string entityType,
        string entityName,
        int indentLevel = 0
    )
    {
        WriteDifference(consoleOutput, difference, $"{entityType}: {entityName}", indentLevel);
    }
}
