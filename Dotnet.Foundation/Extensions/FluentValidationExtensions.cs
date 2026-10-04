using FluentValidation.Results;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for checking the state of <see cref = "ValidationResult" /> instances.
/// </summary>
public static class FluentValidationExtensions
{
    extension(ValidationResult validationResult)
    {
        public bool IsSuccess => validationResult.IsValid;

        public bool IsFailure => !validationResult.IsValid;
    }
}
