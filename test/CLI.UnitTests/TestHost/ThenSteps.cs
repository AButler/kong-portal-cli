using System.IO.Abstractions;
using FluentAssertions;

namespace CLI.UnitTests.TestHost;

public class ThenSteps(IFileSystem fileSystem)
{
    public void DirectoryShouldExist(string path)
    {
        fileSystem.Directory.Exists(path).Should().BeTrue($"directory does not exist: {path}");
    }

    public void FileShouldExist(string path)
    {
        fileSystem.File.Exists(path).Should().BeTrue($"file does not exist: {path}");
    }

    public void FileContentsShouldBe(string path, string contents)
    {
        fileSystem.File.ReadAllText(path).Should().Be(contents);
    }
}
