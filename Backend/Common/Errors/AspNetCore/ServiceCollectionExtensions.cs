using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Backend.Common.Errors.AspNetCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers error handling services into the application's dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureErrors">Configuration callback for <see cref="ErrorsOptions"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddErrors(
        this IServiceCollection services,
        Action<OptionsBuilder<ErrorsOptions>> configureErrors)
    {
        configureErrors(services.AddOptions<ErrorsOptions>());
        return services
            .AddProblemDetails()
            .AddSingleton<IProblemDetailsFactory, ProblemDetailsFactory>();
    }
}
