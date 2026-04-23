using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Backend.Dotnet.Application.AIChat.AIModelProvider;
using Backend.Dotnet.Common.Errors.Types;

namespace Backend.Dotnet.Tests.Unit.Applications.AIChat;

public class AIProviderFactoryTests
{
    private readonly Mock<ILogger<AIProviderFactory>> _mockLogger;

    public AIProviderFactoryTests()
    {
        _mockLogger = new Mock<ILogger<AIProviderFactory>>();
    }

    [Fact]
    public void GetClient_ReturnsMockClient_WhenProviderIsMock()
    {
        // ARRANGE
        var mockClient = new Mock<IAiClient>();
        mockClient.Setup(c => c.ProviderName).Returns("Mock");

        var clients = new List<IAiClient> { mockClient.Object };
        var factory = new AIProviderFactory(clients, _mockLogger.Object);

        // ACT
        var result = factory.GetClient("Mock");

        // ASSERT
        Assert.True(result.IsSucceeded);
        Assert.Equal("Mock", result.Value!.ProviderName);
    }

    [Fact]
    public void GetClient_ReturnsOpenAiClient_WhenProviderIsOpenAi()
    {
        // ARRANGE
        var openAi = new Mock<IAiClient>();
        openAi.Setup(c => c.ProviderName).Returns("OpenAi");

        var clients = new List<IAiClient> { openAi.Object };
        var factory = new AIProviderFactory(clients, _mockLogger.Object);

        // ACT
        var result = factory.GetClient("OpenAi");

        // ASSERT
        Assert.True(result.IsSucceeded);
        Assert.Equal("OpenAi", result.Value!.ProviderName);
    }

    [Fact]
    public void GetClient_ReturnsOpenRouterClient_WhenProviderIsOpenRouter()
    {
        // ARRANGE
        var openRouter = new Mock<IAiClient>();
        openRouter.Setup(c => c.ProviderName).Returns("OpenRouter");

        var clients = new List<IAiClient> { openRouter.Object };
        var factory = new AIProviderFactory(clients, _mockLogger.Object);

        // ACT
        var result = factory.GetClient("OpenRouter");

        // ASSERT
        Assert.True(result.IsSucceeded);
        Assert.Equal("OpenRouter", result.Value!.ProviderName);
    }

    [Fact]
    public void GetClient_ReturnsError_WhenRequestedProviderNotFound()
    {
        // ARRANGE
        var mockClient = new Mock<IAiClient>();
        mockClient.Setup(c => c.ProviderName).Returns("Mock");

        var clients = new List<IAiClient> { mockClient.Object };
        var factory = new AIProviderFactory(clients, _mockLogger.Object);

        // ACT
        var result = factory.GetClient("NonExistent");

        // ASSERT
        Assert.False(result.IsSucceeded);
        Assert.IsType<ProviderNotFoundError>(result.Error);
    }

    [Fact]
    public void GetClient_ReturnsError_WhenNoClientsRegistered()
    {
        // ARRANGE
        var clients = new List<IAiClient>();
        var factory = new AIProviderFactory(clients, _mockLogger.Object);

        // ACT
        var result = factory.GetClient("OpenAi");

        // ASSERT
        Assert.False(result.IsSucceeded);
        Assert.IsType<ProviderNotFoundError>(result.Error);
    }
}
