﻿using System.Text.RegularExpressions;

namespace Kong.Portal.CLI;

internal static class DataUriHelpers
{
    private static readonly Regex DataUriRegex = new(@"^data:image\/(png|jpeg|x-icon|ico|icon|vnd.microsoft.icon|gif);base64,(?<Data>.*)$");

    public static byte[] GetData(string dataUri)
    {
        var match = DataUriRegex.Match(dataUri);
        if (!match.Success)
        {
            throw new ArgumentException("Invalid data URI", nameof(dataUri));
        }

        var data = match.Groups["Data"].Value;

        return Convert.FromBase64String(data);
    }
}
