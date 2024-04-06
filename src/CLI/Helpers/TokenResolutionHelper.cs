namespace Kong.Portal.CLI;

internal static class TokenResolutionHelper
{
    public static string ResolveToken(string? token, FileInfo? tokenFile)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            return token;
        }

        if (tokenFile != null && tokenFile.Exists)
        {
            return File.ReadAllText(tokenFile.FullName);
        }

        throw new TokenNotFoundException();
    }
}
