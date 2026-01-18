using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend.Dotnet.Application.AIChat.AIModelProvider.OpenRouter;
using Backend.Dotnet.Application.AIChat.Configuration;
using Backend.Dotnet.Common.Errors.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Backend.Dotnet.Tests.Unit.Applications.AIChat;

/// <summary>
/// Unit tests for OpenRouterClient implementation.
/// Tests all behaviors: success, HTTP errors, empty prompts, exceptions, and cancellation.
/// </summary>
public sealed class OpenRouterClientTests
{
    private readonly Mock<ILogger<OpenRouterClient>> _mockLogger;
    private readonly IOptions<AIChatOptions> _options;

    public OpenRouterClientTests()
    {
        _mockLogger = new Mock<ILogger<OpenRouterClient>>();
        _options = Options.Create(new AIChatOptions
        {
            ProviderInfo =
            {
                ["OpenRouter"] = new AiProviderOptions
                {
                    BaseUrl = "http://localhost/",
                    ApiKey = "test-key",
                    Model = "test-model",
                    MaxTokens = 100,
                    TimeoutSeconds = 30
                }
            }
        });
    }

    [Fact]
    public async Task QueryAsync_ReturnsSuccessResult_WhenApiReturnsValidResponse()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"This is a test response\"}}]}",
                    Encoding.UTF8,
                    "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var client = new OpenRouterClient(httpClient, _options, _mockLogger.Object);

        // Act
        var result = await client.QueryAsync("Test prompt");

        // Assert
        Assert.True(result.IsSucceeded);
        Assert.NotNull(result.Value);
        Assert.Equal("This is a test response", result.Value);
    }

    [Fact]
    public async Task QueryAsync_SendsCorrectPayload_ToOpenRouterApi()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
            {
                capturedRequest = req;
                capturedBody = req.Content?.ReadAsStringAsync(ct).GetAwaiter().GetResult();

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Response\"}}]}",
                        Encoding.UTF8,
                        "application/json")
                };
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var client = new OpenRouterClient(httpClient, _options, _mockLogger.Object);

        // Act
        await client.QueryAsync("Explain graph databases");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.EndsWith("v1/chat/completions", capturedRequest.RequestUri!.ToString());

        Assert.NotNull(capturedBody);
        Assert.Contains("\"model\":\"test-model\"", capturedBody!);
        Assert.Contains("\"max_tokens\":100", capturedBody!);
        Assert.Contains("\"role\":\"user\"", capturedBody!);
        Assert.Contains("Explain graph databases", capturedBody!);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "Invalid request")]
    [InlineData(HttpStatusCode.Unauthorized, "Unauthorized")]
    [InlineData(HttpStatusCode.InternalServerError, "Internal Server Error")]
    public async Task QueryAsync_ReturnsAiProviderError_WhenApiReturnsErrorStatus(HttpStatusCode statusCode, string content)
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "text/plain")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var client = new OpenRouterClient(httpClient, _options, _mockLogger.Object);

        // Act
        var result = await client.QueryAsync("Test prompt");

        // Assert
        Assert.False(result.IsSucceeded);
        Assert.NotNull(result.Error);
        Assert.IsType<AiProviderError>(result.Error);
        Assert.Contains(statusCode.ToString(), result.Error.Detail);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task QueryAsync_ThrowsArgumentException_WhenPromptIsInvalid(string? invalidPrompt)
    {
        // Arrange
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var client = new OpenRouterClient(httpClient, _options, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await client.QueryAsync(invalidPrompt!));
    }

    [Fact]
    public async Task QueryAsync_ReturnsAiProviderError_WhenHttpClientThrowsException()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var client = new OpenRouterClient(httpClient, _options, _mockLogger.Object);

        // Act
        var result = await client.QueryAsync("Test prompt");

        // Assert
        Assert.False(result.IsSucceeded);
        Assert.NotNull(result.Error);
        Assert.IsType<AiProviderError>(result.Error);
        Assert.Contains("Network error", result.Error.Detail);
    }

    [Fact]
    public async Task QueryAsync_ReturnsAiProviderError_WhenJsonParsingFails()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Invalid JSON", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var client = new OpenRouterClient(httpClient, _options, _mockLogger.Object);

        // Act
        var result = await client.QueryAsync("Test prompt");

        // Assert
        Assert.False(result.IsSucceeded);
        Assert.NotNull(result.Error);
        Assert.IsType<AiProviderError>(result.Error);
    }

    [Fact]
    public async Task QueryAsync_HonorsCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var client = new OpenRouterClient(httpClient, _options, _mockLogger.Object);

        // Act
        var result = await client.QueryAsync("Test prompt", cts.Token);

        // Assert
        Assert.False(result.IsSucceeded);
        Assert.NotNull(result.Error);
        Assert.IsType<AiProviderError>(result.Error);
    }

    [Fact]
    public async Task QueryAsync_ReturnsEmptyString_WhenContentIsNull()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":null}}]}",
                    Encoding.UTF8,
                    "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var client = new OpenRouterClient(httpClient, _options, _mockLogger.Object);

        // Act
        var result = await client.QueryAsync("Test prompt");

        // Assert
        Assert.True(result.IsSucceeded);
        Assert.NotNull(result.Value);
        Assert.Equal(string.Empty, result.Value);
    }

    [Fact]
    public void ProviderName_ReturnsOpenRouter()
    {
        // Arrange
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var client = new OpenRouterClient(httpClient, _options, _mockLogger.Object);

        // Act & Assert
        Assert.Equal("OpenRouter", client.ProviderName);
    }
}
