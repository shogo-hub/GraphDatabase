using Backend.WebApi.RagIntegration.Models;

namespace Backend.Application.Rag;

/// <summary>
/// Pure, testable mapper for RAG models.
/// </summary>
internal static class RagMapper
{
    /// <summary>
    /// Map API request to domain model.
    /// </summary>
    public static RagDomainModel ToDomain(RagQueryRequest request) =>
        new()
        {
            Query = request.Query.Trim(),
            Context = request.Context?.Trim(),
            TaskType = request.TaskType.ToLowerInvariant(),
            Provider = request.Provider.ToString()
        };

    /// <summary>
    /// Map AI response string to domain result model.
    /// </summary>
    public static RagResultModel FromAi(string aiResponse) =>
        new()
        {
            Output = (aiResponse ?? string.Empty).Trim()
        };
}