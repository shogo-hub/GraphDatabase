using System.Text.Json.Serialization;

namespace Backend.Dotnet.Application.AIChat.PromptCreator;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PromptTemplateType
{
    Explain,
}