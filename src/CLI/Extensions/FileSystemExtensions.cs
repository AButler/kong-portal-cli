using System.IO.Abstractions;

namespace Kong.Portal.CLI;

internal static class FileSystemExtensions
{
    extension(IDirectory directory)
    {
        public void EnsureDirectory(string path)
        {
            if (directory.Exists(path))
            {
                return;
            }

            directory.CreateDirectory(path);
        }
    }

    extension(IFile file)
    {
        public async Task WriteDataUriImage(string path, string imageData, CancellationToken cancellationToken = default)
        {
            var bytes = DataUriHelpers.GetData(imageData);

            await file.WriteAllBytesAsync(path, bytes, cancellationToken);
        }
    }
}
