namespace Backend.Dotnet.Common.Mediator;

internal sealed class RequestHandlerRegistry(IEnumerable<RequestHandlerDescriptor> descriptors)
{
    private readonly Dictionary<(Type RequestType, Type ResponseType), (Type HandlerType, Type FilterType)> _types
        = descriptors.ToDictionary(x => (x.RequestType, x.ResponseType), x => (x.HandlerType, x.FilterType));

    public (Type HandlerType, Type FilterType) GetTypes(Type requestType, Type responseType)
    {
        return _types[(requestType, responseType)];
    }
}