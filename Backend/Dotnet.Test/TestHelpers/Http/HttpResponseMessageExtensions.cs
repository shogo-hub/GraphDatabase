using Backend.Dotnet.Common.Serialization.Json;
using System.Text.Json;
using UnitType = Backend.Dotnet.Common.Miscellaneous.Unit;

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
            x => JsonSerializer.Deserialize<T>(x, ControllerApiJsonSerializer.Options)
                ?? throw new InvalidOperationException("Content deserialized to null."));
    }

    public static async Task<ApiResponse<UnitType>> ToEmptyApiResponseAsync(
        this HttpResponseMessage message,
        CancellationToken cancellationToken)
    {
        var content = await message.Content.ReadAsStringAsync(cancellationToken);
        return new ApiResponse<UnitType>(message.StatusCode, content, message.Headers, _ => UnitType.Value);
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