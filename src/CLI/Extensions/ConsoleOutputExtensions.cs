using Kong.Portal.CLI.Services;
using Kong.Portal.CLI.Services.Models;

namespace Kong.Portal.CLI;

internal static class ConsoleOutputExtensions
{
    extension(IConsoleOutput consoleOutput)
    {
        public void WriteDifference<T>(Difference<T> difference, string entityName, int indentLevel = 0)
        {
            var spaces = new string(' ', indentLevel * 2);

            consoleOutput.WriteLine($"{spaces}{difference.DifferenceType.ToSymbol()} {entityName}");
        }

        public void WriteDifference<T>(Difference<T> difference, string entityType, string entityName, int indentLevel = 0)
        {
            consoleOutput.WriteDifference(difference, $"{entityType}: {entityName}", indentLevel);
        }
    }
}
