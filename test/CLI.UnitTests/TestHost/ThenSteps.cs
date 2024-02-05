namespace CLI.UnitTests.TestHost;

public class ThenSteps(DumpedFileSteps dumpedFile)
{
    public DumpedFileSteps DumpedFile { get; } = dumpedFile;
}
