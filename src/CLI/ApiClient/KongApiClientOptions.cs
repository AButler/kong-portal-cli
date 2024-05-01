namespace Kong.Portal.CLI.ApiClient;

internal record KongApiClientOptions(string Token, string BaseUrl, string Region = "", bool DebugLoggingEnabled = false);
