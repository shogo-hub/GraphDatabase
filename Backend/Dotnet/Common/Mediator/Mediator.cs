using Microsoft.Extensions.DependencyInjection;

namespace Backend.Dotnet.Common.Mediator;

internal sealed class Mediator(
    IServiceProvider serviceProvider,
    RequestHandlerRegistry requestHandlerRegistry) :
    IMediator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly RequestHandlerRegistry _requestHandlerRegistry = requestHandlerRegistry;

    public async Task<TResponse> DispatchAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var (handlerType, filterType) = _requestHandlerRegistry.GetTypes(typeof(TRequest), typeof(TResponse));
        var handler = (IRequestHandler<TRequest, TResponse>)_serviceProvider.GetRequiredService(handlerType);
        var filters = (IEnumerable<IRequestHandlerDecorator<TRequest, TResponse>>)_serviceProvider.GetServices(
            filterType);
        var combinedHandler = filters
            .Aggregate(handler, (inner, filter) => new DecoratingRequestHandler<TRequest, TResponse>(inner, filter));
        return await combinedHandler.HandleAsync(request, cancellationToken);
    }

    private sealed class DecoratingRequestHandler<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> inner,
        IRequestHandlerDecorator<TRequest, TResponse> decorator) :
        IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            return await decorator.HandleAsync(inner, request, cancellationToken);
        }
    }
}