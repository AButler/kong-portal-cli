using Microsoft.Extensions.Options;

namespace Kong.Portal.CLI.Config;

internal class KongOptionsValidator : IValidateOptions<KongOptions>
{
    public ValidateOptionsResult Validate(string? name, KongOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Token))
        {
            return ValidateOptionsResult.Fail("Kong API Token is required");
        }

        return ValidateOptionsResult.Success;
    }
}
