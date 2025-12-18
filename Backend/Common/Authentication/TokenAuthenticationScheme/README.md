# Authentication pipeline

# General
### Core Handler & Options
1. TokenAuthenticationHandler.cs
2. TokenAuthenticationOptions.cs
3. TokenAuthenticationEvents.cs

### Interfaces
1. ITokenAccessor.cs
2. ITokenGenerator.cs
3. ITokenParser.cs

### Contexts(delegator) & Models
1. TokenSigningInContext.cs
2. TokenChallengeContext.cs
3. TokenForbiddenContext.cs
4. Token.cs
5. TokenGenerationResult.cs
6. TokenParseResult.cs 

# Cookies

### Tool to managing accessing data from cookies
1. AccessTokenCookie.cs
2. CookieTokenAccessor.cs
3. CookieTokenAccessorOptions.cs

# Paseto

### Tool to manage Paseto ruled En/Decryption
1. PasetoTokenGenerator.cs
2. PasetoTokenParser.cs

### Register Paseto management tool to DI Container
1. AuthenticationBuilderExtensions.cs

### Misc
1. PasetoTokenOptions.cs
2. PasetoTokenCookieDefault.cs
