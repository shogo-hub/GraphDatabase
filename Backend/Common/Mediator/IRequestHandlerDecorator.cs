namespace Backend.Common.Mediator;

public interface IRequestHandlerDecorator<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(
        IRequestHandler<TRequest, TResponse> inner,
        TRequest request,
        CancellationToken cancellationToken);
}