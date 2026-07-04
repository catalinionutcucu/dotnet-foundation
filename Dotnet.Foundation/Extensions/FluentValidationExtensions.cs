using FluentValidation.Results;

namespace Dotnet.Foundation.Extensions;

public static class FluentValidationExtensions
{
    extension(ValidationResult validationResult)
    {
        public bool IsSuccess => validationResult.IsValid;

        public bool IsFailure => !validationResult.IsValid;
    }
}
