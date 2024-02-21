namespace Kong.Portal.CLI;

internal static class TokenResolutionHelper
{
    public static string ResolveToken(string token, string tokenFile)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            return token;
        }

        if (!string.IsNullOrWhiteSpace(tokenFile) && File.Exists(tokenFile))
        {
            return File.ReadAllText(tokenFile);
        }

        throw new TokenNotFoundException();
    }
}
