using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dotnet.Foundation.Models;

/// <summary>
/// Represents the JSON converter factory for <see cref = "Result{TValue,TError}" /> and <see cref = "Result{TError}" /> instances, converting the state with the value of a success result or the error of a failure result.
/// </summary>
public sealed class ResultJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && (typeToConvert.GetGenericTypeDefinition() == typeof(Result<,>) || typeToConvert.GetGenericTypeDefinition() == typeof(Result<>));
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var resultJsonConverterType = typeToConvert.GetGenericTypeDefinition() == typeof(Result<,>) ?
            typeof(ResultJsonConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments()) :
            typeof(ResultJsonConverter<>).MakeGenericType(typeToConvert.GetGenericArguments());

        return (JsonConverter)Activator.CreateInstance(resultJsonConverterType)!;
    }

    private static string GetPropertyName(string propertyName, JsonSerializerOptions options)
    {
        return options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
    }

    /// <summary>
    /// Represents the JSON converter for <see cref = "Result{TValue,TError}" /> instances.
    /// </summary>
    private sealed class ResultJsonConverter<TValue, TError> : JsonConverter<Result<TValue, TError>>
    {
        /// <inheritdoc />
        public override Result<TValue, TError> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDocument = JsonDocument.ParseValue(ref reader);

            var state = jsonDocument.RootElement.GetProperty(GetPropertyName(nameof(Result<TValue, TError>.State), options)).Deserialize<ResultState>(options);

            return state is ResultState.Success ?
                Result<TValue, TError>.Success(jsonDocument.RootElement.GetProperty(GetPropertyName(nameof(Result<TValue, TError>.Value), options)).Deserialize<TValue>(options)!) :
                Result<TValue, TError>.Failure(jsonDocument.RootElement.GetProperty(GetPropertyName(nameof(Result<TValue, TError>.Error), options)).Deserialize<TError>(options)!);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Result<TValue, TError> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(GetPropertyName(nameof(Result<TValue, TError>.State), options));
            JsonSerializer.Serialize(writer, value.State, options);

            if (value.IsSuccess)
            {
                writer.WritePropertyName(GetPropertyName(nameof(Result<TValue, TError>.Value), options));
                JsonSerializer.Serialize(writer, value.Value, options);
            }
            else
            {
                writer.WritePropertyName(GetPropertyName(nameof(Result<TValue, TError>.Error), options));
                JsonSerializer.Serialize(writer, value.Error, options);
            }

            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// Represents the JSON converter for <see cref = "Result{TError}" /> instances.
    /// </summary>
    private sealed class ResultJsonConverter<TError> : JsonConverter<Result<TError>>
    {
        /// <inheritdoc />
        public override Result<TError> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDocument = JsonDocument.ParseValue(ref reader);

            var state = jsonDocument.RootElement.GetProperty(GetPropertyName(nameof(Result<TError>.State), options)).Deserialize<ResultState>(options);

            return state is ResultState.Success ?
                Result<TError>.Success() :
                Result<TError>.Failure(jsonDocument.RootElement.GetProperty(GetPropertyName(nameof(Result<TError>.Error), options)).Deserialize<TError>(options)!);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Result<TError> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(GetPropertyName(nameof(Result<TError>.State), options));
            JsonSerializer.Serialize(writer, value.State, options);

            if (value.IsFailure)
            {
                writer.WritePropertyName(GetPropertyName(nameof(Result<TError>.Error), options));
                JsonSerializer.Serialize(writer, value.Error, options);
            }

            writer.WriteEndObject();
        }
    }
}
