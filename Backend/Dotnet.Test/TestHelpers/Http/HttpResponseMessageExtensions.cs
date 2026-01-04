using Backend.Common.Miscellaneous;
using Backend.Common.Serialization.Json;
using System.Text.Json;

namespace Backend.Dotnet.Tests.TestHelpers.Http;

public static class HttpResponseMessageExtensions
{
    public static async Task<ApiResponse<T>> ToJsonApiResponseAsync<T>(
        this HttpResponseMessage message,
        CancellationToken cancellationToken)
    {
        var content = await message.Content.ReadAsStringAsync(cancellationToken);
        return new ApiResponse<T>(
            message.StatusCode,
            content,
            message.Headers,
            x => JsonSerializer.Deserialize<T>(x, WebApiJsonSerializer.Options)
                ?? throw new InvalidOperationException("Content deserialized to null."));
    }

    public static async Task<ApiResponse<Unit>> ToEmptyApiResponseAsync(
        this HttpResponseMessage message,
        CancellationToken cancellationToken)
    {
        var content = await message.Content.ReadAsStringAsync(cancellationToken);
        return new ApiResponse<Unit>(message.StatusCode, content, message.Headers, _ => Unit.Value);
    }

    public static async Task<ApiResponse<T>> ToEnumApiResponseAsync<T>(
        this HttpResponseMessage message,
        CancellationToken cancellationToken)
        where T : struct, Enum
    {
        var content = await message.Content.ReadAsStringAsync(cancellationToken);
        return new ApiResponse<T>(message.StatusCode, content, message.Headers, Enum.Parse<T>);
    }
}