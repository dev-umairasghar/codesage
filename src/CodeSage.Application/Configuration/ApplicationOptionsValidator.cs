using CodeSage.Application.Configuration;
using Microsoft.Extensions.Options;

namespace CodeSage.Application.Configuration;

/// <summary>
/// Validates application identity options.
/// </summary>
public sealed class ApplicationOptionsValidator : IValidateOptions<ApplicationOptions>
{
    public ValidateOptionsResult Validate(string? name, ApplicationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Name))
        {
            failures.Add("Application:Name is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Version))
        {
            failures.Add("Application:Version is required (example: 0.1.0).");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
