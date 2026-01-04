using AutoFixture;
using Backend.Common.Authentication.TokenAuthenticationScheme.Cookies;
using Backend.Dotnet.Tests.TestHelpers;
using Backend.Dotnet.Tests.TestHelpers.Http;
using Backend.Dotnet.Tests.TestHelpers.Users;
using Backend.Controller.Users.Models;
using IntegrationMocks.Core;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace Backend.Dotnet.Tests.Component.Users;

public sealed class AuthTests : IClassFixture<BackendServiceFixture>, IDisposable
{
    private readonly IFixture _fixture;
    private readonly IInfrastructureService<BackendContract> _backend;
    private readonly TestHttpClient _httpClient;
    private readonly BackendAuthTestClient _authClient;
    private readonly BackendSchoolsTestClient _schoolsClient;
    private readonly BackendUsersTestClient _usersClient;

    public AuthTests(BackendServiceFixture backendFixture)
    {
        _fixture = new Fixture();
        _backend = backendFixture.Backend;
        _httpClient = TestHttpClientFactory.Create(_backend.Contract.ApiUrl);
        _authClient = new BackendAuthTestClient(_httpClient);
        _schoolsClient = new BackendSchoolsTestClient(_httpClient);
        _usersClient = new BackendUsersTestClient(_httpClient);
    }

    public void Dispose()
    {
        _usersClient?.Dispose();
        _schoolsClient?.Dispose();
        _authClient?.Dispose();
        _httpClient?.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_creates_user()
    {
        // ARRANGE
        var createSchoolRequest = _fixture.Create<CreateSchoolRequest>();
        var school = (await _schoolsClient.CreateAsync(createSchoolRequest)).GetValueOrThrow();

        // ACT
        var registerUserRequest = _fixture.Build<RegisterUserRequest>().With(x => x.SchoolId, school.Id).Create();
        var user = (await _authClient.RegisterAsync(registerUserRequest)).GetValueOrThrow();

        // ASSERT
        Assert.Equal(registerUserRequest.UserName, user.UserName);
        Assert.Equal(registerUserRequest.FullName, user.FullName);
        Assert.Equal(registerUserRequest.SchoolId, user.SchoolId);
        Assert.Equal(registerUserRequest.Role, user.Roles.Single());
    }

    [Fact]
    public async Task LoginAsync_sets_access_token_cookie()
    {
        // ARRANGE
        var createSchoolRequest = _fixture.Create<CreateSchoolRequest>();
        var school = (await _schoolsClient.CreateAsync(createSchoolRequest)).GetValueOrThrow();
        var registerUserRequest = _fixture.Build<RegisterUserRequest>().With(x => x.SchoolId, school.Id).Create();
        var user = (await _authClient.RegisterAsync(registerUserRequest)).GetValueOrThrow();

        // ACT
        var loginRequest = new LoginRequest
        {
            UserName = registerUserRequest.UserName,
            SchoolId = registerUserRequest.SchoolId,
            Pwd = registerUserRequest.Pwd
        };
        var loginResponse = await _authClient.LoginAsync(loginRequest);

        // ASSERT
        var setCookie = loginResponse.Headers.Single(x => x.Key == HeaderNames.SetCookie).Value;
        Assert.Contains(setCookie, x =>
        {
            var (key, value) = ParseCookie(x);
            return key == AccessTokenCookie.DefaultName && value != null;
        });
    }

    [Fact]
    public async Task LogoutAsync_removes_access_token_cookie()
    {
        // ARRANGE
        var createSchoolRequest = _fixture.Create<CreateSchoolRequest>();
        var school = (await _schoolsClient.CreateAsync(createSchoolRequest)).GetValueOrThrow();
        var registerUserRequest = _fixture.Build<RegisterUserRequest>().With(x => x.SchoolId, school.Id).Create();
        var user = (await _authClient.RegisterAsync(registerUserRequest)).GetValueOrThrow();
        var loginRequest = new LoginRequest
        {
            UserName = registerUserRequest.UserName,
            SchoolId = registerUserRequest.SchoolId,
            Pwd = registerUserRequest.Pwd
        };
        (await _authClient.LoginAsync(loginRequest)).GetValueOrThrow();

        // ACT
        var logoutResponse = await _authClient.LogoutAsync();

        // ASSERT
        var setCookie = logoutResponse.Headers.Single(x => x.Key == HeaderNames.SetCookie).Value;
        Assert.Contains(setCookie, x =>
        {
            var (key, value) = ParseCookie(x);
            return key == AccessTokenCookie.DefaultName && value == null;
        });
    }

    [Fact]
    public async Task Current_user_endpoint_is_inaccessible_by_default()
    {
        var response = await _usersClient.GetCurrentAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
    }

    [Fact]
    public async Task LoginAsync_makes_current_user_endpoint_accessible()
    {
        // ARRANGE
        var createSchoolRequest = _fixture.Create<CreateSchoolRequest>();
        var school = (await _schoolsClient.CreateAsync(createSchoolRequest)).GetValueOrThrow();
        var registerUserRequest = _fixture.Build<RegisterUserRequest>().With(x => x.SchoolId, school.Id).Create();
        var user = (await _authClient.RegisterAsync(registerUserRequest)).GetValueOrThrow();

        // ACT
        var loginRequest = new LoginRequest
        {
            UserName = registerUserRequest.UserName,
            SchoolId = registerUserRequest.SchoolId,
            Pwd = registerUserRequest.Pwd
        };
        (await _authClient.LoginAsync(loginRequest)).GetValueOrThrow();
        var currentUser = (await _usersClient.GetCurrentAsync()).GetValueOrThrow();

        // ASSERT
        Assert.Equal(user.Id, currentUser.Id);
        Assert.Equal(user.UserName, currentUser.UserName);
        Assert.Equal(user.FullName, currentUser.FullName);
        Assert.Equal(user.SchoolId, currentUser.SchoolId);
        Assert.Equal(user.Roles, currentUser.Roles);
    }

    [Fact]
    public async Task LogoutAsync_makes_current_user_endpoint_inaccessible()
    {
        // ARRANGE
        var createSchoolRequest = _fixture.Create<CreateSchoolRequest>();
        var school = (await _schoolsClient.CreateAsync(createSchoolRequest)).GetValueOrThrow();
        var registerUserRequest = _fixture.Build<RegisterUserRequest>().With(x => x.SchoolId, school.Id).Create();
        var user = (await _authClient.RegisterAsync(registerUserRequest)).GetValueOrThrow();
        var loginRequest = new LoginRequest
        {
            UserName = registerUserRequest.UserName,
            SchoolId = registerUserRequest.SchoolId,
            Pwd = registerUserRequest.Pwd
        };
        (await _authClient.LoginAsync(loginRequest)).GetValueOrThrow();

        // ACT
        (await _authClient.LogoutAsync()).GetValueOrThrow();
        var currentUserResponse = await _usersClient.GetCurrentAsync();

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, currentUserResponse.Status);
    }

    private static KeyValuePair<string, string?> ParseCookie(string setCookie)
    {
        var tokens = setCookie.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var assignment = tokens[0];
        var assignmentTokens = assignment.Split('=', StringSplitOptions.RemoveEmptyEntries);
        return KeyValuePair.Create(assignmentTokens[0], assignmentTokens.Length > 1 ? assignmentTokens[1] : null);
    }
}