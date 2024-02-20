using Kong.Portal.CLI.Services.Models;
using Pastel;

namespace Kong.Portal.CLI;

internal static class DifferenceExtensions
{
    public static string ToSymbol(this DifferenceType differenceType)
    {
        return differenceType switch
        {
            DifferenceType.NoChange => "#".Pastel(ConsoleColor.Gray),
            DifferenceType.Add => "+".Pastel(ConsoleColor.Green),
            DifferenceType.Update => "~".Pastel(ConsoleColor.Yellow),
            DifferenceType.Delete => "-".Pastel(ConsoleColor.Red),
            _ => throw new ArgumentOutOfRangeException(nameof(differenceType), differenceType, null)
        };
    }
}
