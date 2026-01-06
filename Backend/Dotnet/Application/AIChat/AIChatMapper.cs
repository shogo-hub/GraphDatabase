using Backend.Dotnet.Controllers.AIChat.Models;

namespace Backend.Dotnet.Application.AIChat;

/// <summary>
/// Pure, testable mapper for AIChat models.
/// </summary>
internal static class AIChatMapper
{
    /// <summary>
    /// Map API request to domain model.
    /// </summary>
    public static AIChatDomainModel ToDomain(AIChatQueryRequest request) =>
        new()
        {
            Query = request.Query.Trim(),
            Context = request.Context?.Trim(),
            TaskType = request.TaskType,
            Provider = request.Provider.ToString()
        };

    /// <summary>
    /// Map AI response string to domain result model.
    /// </summary>
    public static AIChatResultModel FromAi(string aiResponse) =>
        new()
        {
            Output = (aiResponse ?? string.Empty).Trim()
        };
}