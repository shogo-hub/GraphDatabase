using Backend.Dotnet.Common.Miscellaneous;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Dotnet.Common.Mediator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        return services
            .AddSingleton<RequestHandlerRegistry>()
            .AddScoped<IMediator, Mediator>();
    }

    public static IServiceCollection AddHandler<TRequest, TResponse, THandler>(this IServiceCollection services)
        where TRequest : IRequest<TResponse>
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        return services
            .AddScoped<IRequestHandler<TRequest, TResponse>, THandler>()
            .AddSingleton(RequestHandlerDescriptor.Create<TRequest, TResponse>());
    }

    public static IServiceCollection AddHandler<TRequest, THandler>(this IServiceCollection services)
        where TRequest : IRequest<Unit>
        where THandler : class, IRequestHandler<TRequest, Unit>
    {
        return AddHandler<TRequest, Unit, THandler>(services);
    }
}