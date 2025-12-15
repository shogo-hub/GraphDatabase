namespace Backend.Common.Errors.Types;

public sealed class EntityNotFoundError : Error
{
    public EntityNotFoundError(string detail, string entityType, string entityId)
        : base("common.entity_not_found", detail, new Params
        {
            EntityType = entityType,
            EntityId = entityId
        })
    {
    }

    public override string Title => "Entity not found";

    public override Params Parameters => (Params)base.Parameters!;

    public sealed class Params
    {
        public required string EntityType { get; init; }

        public required string EntityId { get; init; }
    }
}