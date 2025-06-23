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

    public static async Task WriteDataUriImage(this IFile file, string path, string imageData, CancellationToken cancellationToken = default)
    {
        var bytes = DataUriHelpers.GetData(imageData);

        await file.WriteAllBytesAsync(path, bytes, cancellationToken);
    }
}
