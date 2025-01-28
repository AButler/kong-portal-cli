using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace Kong.Portal.CLI;

internal static class DataUriHelpers
{
    private static readonly Regex DataUriRegex = new(@"^data:image\/(png|jpeg|x-icon|ico|icon|vnd.microsoft.icon|gif);base64,(?<Data>.*)$");

    private static readonly Dictionary<string, string> SupportedFormats = new()
    {
        [".jpg"] = "jpeg",
        [".jpeg"] = "jpeg",
        [".png"] = "png",
        [".gif"] = "gif",
    };

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

    public static string? ToDataUri(string filename, byte[] imageStream)
    {
        var extension = Path.GetExtension(filename);

        if (!SupportedFormats.TryGetValue(extension, out var type))
        {
            return null;
        }

        var base64ImageData = Convert.ToBase64String(imageStream);
        return $"data:image/{type};base64,{base64ImageData}";
    }
}
