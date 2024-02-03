using System.IO.Abstractions;

namespace Kong.Portal.CLI;

internal static class FileSystemExtensions
{
    public static void EnsureDirectory(this IDirectory directory, string path)
    {
        if (directory.Exists(path))
        {
            return;
        }

        directory.CreateDirectory(path);
    }
}
