using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Common.Serialization.Json;

/// <summary>
/// Provides a preconfigured <see cref="JsonSerializerOptions"/> instance for
/// ASP.NET Core Web API usage. The options enable web-friendly defaults and
/// register common converters used across the application.
/// </summary>
public static class WebApiJsonSerializer
{
    /// <summary>
    /// Gets a shared <see cref="JsonSerializerOptions"/> configured with
    /// <see cref="JsonSerializerOptions.Web"/> defaults and application-wide converters.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = JsonSerializerOptions.Web
        .With(x => x.Converters.Add(new JsonStringEnumConverter()))
        /*.With(x => x.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower)*/;
}