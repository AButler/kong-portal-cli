namespace Kong.Portal.CLI.Helpers;

public static class DirectoryHelpers
{
    public static void EnsureDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            return;
        }

        var parentDirectory = Path.GetDirectoryName(directory);
        if (parentDirectory != null && !Directory.Exists(parentDirectory))
        {
            EnsureDirectory(parentDirectory);
        }

        Directory.CreateDirectory(directory);
    }
}
