namespace Backend.Application.Rag;

/// <summary>
/// Domain model representing a RAG query request.
/// </summary>
public sealed record RagDomainModel
{
    public required string Query { get; init; }
    public string? Context { get; init; }
    public required string TaskType { get; init; }
    public required string Provider { get; init; }
}

/// <summary>
/// Domain model representing a RAG query result.
/// </summary>
public sealed record RagResultModel
{
    public required string Output { get; init; }
}