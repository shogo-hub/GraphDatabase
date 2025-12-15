namespace Backend.Common.Mediator;

public interface IMediator
{
    Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>;
}