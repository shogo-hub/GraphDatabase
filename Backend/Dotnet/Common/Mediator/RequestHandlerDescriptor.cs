namespace Backend.Dotnet.Common.Mediator;

internal sealed class RequestHandlerDescriptor
{
    private RequestHandlerDescriptor(Type requestType, Type responseType, Type handlerType, Type filterType)
    {
        RequestType = requestType;
        ResponseType = responseType;
        HandlerType = handlerType;
        FilterType = filterType;
    }

    public Type RequestType { get; }

    public Type ResponseType { get; }

    public Type HandlerType { get; }

    public Type FilterType { get; }

    public static RequestHandlerDescriptor Create<TRequest, TResponse>() where TRequest : IRequest<TResponse>
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(typeof(TRequest), typeof(TResponse));
        var filterType = typeof(IRequestHandlerDecorator<,>).MakeGenericType(typeof(TRequest), typeof(TResponse));
        return new RequestHandlerDescriptor(typeof(TRequest), typeof(TResponse), handlerType, filterType);
    }
}