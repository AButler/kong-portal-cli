namespace Kong.Portal.CLI.Services.Models;

internal record PageMetadata(int Total, int Size, int Number)
{
    public bool HasMore() => Number * Size < Total;
}