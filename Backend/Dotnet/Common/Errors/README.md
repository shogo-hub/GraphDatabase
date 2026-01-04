
# Errors

This folder centralizes the application's domain error types and the ASP.NET Core
integration that converts those errors into HTTP Problem Details (RFC 7807).

Purpose
- Keep domain/validation/auth/authz error types in one place.
- Provide a single, configurable place to control how those errors are presented to HTTP clients.

Used Design pattern - Factory pattern

![Factory pattern diagram](../../images/Factory%20pattern.png)

Key files and responsibilities
- `Types/` — concrete, domain-neutral error types (e.g. `Error.cs`, `EntityNotFoundError.cs`, `ValidationFailedError.cs`, `AuthenticationFailedError.cs`).
- `ProblemDetailsFactory.cs` — the concrete implementation that converts a domain `Error` into an ASP.NET Core `ProblemDetails` (decides status code, detail fields, etc.).
- `IProblemDetailsFactory.cs` — abstraction for the factory so the conversion can be mocked or replaced.
- `ServiceCollectionExtensions.cs` — DI helper: registers the required services. Specifically, `AddErrors(...)` calls `AddProblemDetails()` and registers `ProblemDetailsFactory` as the `IProblemDetailsFactory` implementation (singleton).
- `ProblemDetailsExtensions.cs` / `PrintableProblemDetails.cs` — helpers for creating and serializing `ProblemDetails` instances.

How it works (simple flow)
1. Domain or application code returns a typed `Error` (or `FailedResult<ErrorType>`).
2. The Web layer (controller/middleware) asks the `IProblemDetailsFactory` to convert the `Error` into a `ProblemDetails`.
3. The resulting `ProblemDetails` is returned to the client (with the appropriate HTTP status code).

Important note about the factory and DI
- `ProblemDetailsFactory` is the concrete creator that implements the conversion logic from `Error` → `ProblemDetails`.
- It is registered into the application's DI container by `ServiceCollectionExtensions.AddErrors(...)`, which does the equivalent of:

```csharp
services.AddProblemDetails();
services.AddSingleton<IProblemDetailsFactory, ProblemDetailsFactory>();
```

This means the factory is available via constructor injection anywhere you need to convert domain errors to HTTP `ProblemDetails`.

Conventions & guidance
- Prefer returning structured error values (e.g. `FailedResult<TError>` / `TryResult`) instead of throwing for expected failures.
- Add a distinct error type for each client-facing failure scenario so the API can map it to an appropriate HTTP status code and payload.
- Keep presentation concerns (HTTP/ProblemDetails) in `AspNetCore` or the factory layer; domain types in `Types/` should remain ASP.NET-agnostic.

Suggested improvements
- Add a mapping table: error type → HTTP status code + example `ProblemDetails` payload.
- Add a short code example showing how to call `AddErrors(...)` in `Program.cs` and how a controller returns a `ProblemDetails` for a `FailedResult`.


