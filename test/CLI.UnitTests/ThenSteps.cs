using System.IO.Abstractions;
using FluentAssertions;

namespace CLI.UnitTests;

public class ThenSteps(IFileSystem fileSystem)
{
    public void DirectoryExists(string path)
    {
        fileSystem.Directory.Exists(path).Should().BeTrue();
    }

    public void FileExists(string path)
    {
        fileSystem.File.Exists(path).Should().BeTrue();
    }
}
