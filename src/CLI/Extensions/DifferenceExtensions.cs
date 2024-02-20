using Kong.Portal.CLI.Services.Models;

namespace Kong.Portal.CLI;

internal static class DifferenceExtensions
{
    public static string ToSymbol(this DifferenceType differenceType)
    {
        return differenceType switch
        {
            DifferenceType.NoChange => "#",
            DifferenceType.Add => "+",
            DifferenceType.Update => "~",
            DifferenceType.Delete => "-",
            _ => throw new ArgumentOutOfRangeException(nameof(differenceType), differenceType, null)
        };
    }
}
