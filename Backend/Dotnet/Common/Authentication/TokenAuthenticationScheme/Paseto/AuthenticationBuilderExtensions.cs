using Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme.Paseto;

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddPasetoTokenCookie(
        this AuthenticationBuilder builder,
        string authenticationScheme,
        string? displayName,
        Action<TokenAuthenticationOptions>? configureOptions,
        Action<PasetoTokenOptions>? configurePasetoTokenOptions,
        Action<CookieTokenAccessorOptions>? configureCookieTokenAccessorOptions)
    {
        // 1. 
        builder.Services.Configure(configurePasetoTokenOptions ?? delegate { });
        builder.Services.Configure(configureCookieTokenAccessorOptions ?? delegate { });
        
        // Add Cookie / Pasetto function as a singleton
        builder.Services
            .AddSingleton<ITokenAccessor, CookieTokenAccessor>()
            .AddSingleton<ITokenParser, PasetoTokenParser>()
            .AddSingleton<ITokenGenerator, PasetoTokenGenerator>();

        return builder.AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(
            authenticationScheme,
            displayName,
            configureOptions);
    }

    public static AuthenticationBuilder AddPasetoTokenCookie(
        this AuthenticationBuilder builder,
        string authenticationScheme,
        Action<TokenAuthenticationOptions>? configureOptions = null,
        Action<PasetoTokenOptions>? configurePasetoTokenOptions = null,
        Action<CookieTokenAccessorOptions>? configureCookieTokenAccessorOptions = null)
    {
        return AddPasetoTokenCookie(
            builder,
            authenticationScheme,
            null,
            configureOptions,
            configurePasetoTokenOptions,
            configureCookieTokenAccessorOptions);
    }
}