namespace CLI.UnitTests.TestHost;

internal class GivenSteps(ApiGivenSteps api, FileGivenSteps file)
{
    public ApiGivenSteps Api { get; } = api;

    public FileGivenSteps File { get; } = file;
}
