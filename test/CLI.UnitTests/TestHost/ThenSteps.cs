namespace CLI.UnitTests.TestHost;

internal class ThenSteps(DumpedFileThenSteps dumpedFile, ApiThenSteps api)
{
    public DumpedFileThenSteps DumpedFile { get; } = dumpedFile;

    public ApiThenSteps Api { get; } = api;
}
