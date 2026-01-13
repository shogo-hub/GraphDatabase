using System;
using System.Collections.Generic;
using Xunit;
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
    public void GetClient_ReturnsCorrectClient_WhenProviderExists_CaseInsensitive()
    {
        // ARRANGE
        // Assume a client named "OpenAI" exists
        var mockOpenAi = new Mock<IAiClient>();
        mockOpenAi.Setup(c => c.ProviderName).Returns("OpenAI");

        // Register in the Factory
        var clients = new List<IAiClient> { mockOpenAi.Object };
        var factory = new AIProviderFactory(clients, _mockLogger.Object);

        // ACT
        // Request with lowercase "openai"
        var result = factory.GetClient("openai");

        // ASSERT
        // Ensure success and that the "OpenAI" client is returned
        Assert.True(result.Success);
        Assert.Equal("OpenAI", result.Value.ProviderName);
    }

    [Fact]
    public void GetClient_FallsBackToMock_WhenRequestedProviderNotFound()
    {
        // ARRANGE
        // Situation where only the "Mock" client exists
        var mockMock = new Mock<IAiClient>();
        mockMock.Setup(c => c.ProviderName).Returns("Mock");

        var clients = new List<IAiClient> { mockMock.Object };
        var factory = new AIProviderFactory(clients, _mockLogger.Object);

        // ACT
        // Request a non-existent provider "Gemini"
        var result = factory.GetClient("Gemini");

        // ASSERT
        Assert.True(result.Success);
        // Although Gemini was requested, it should fallback to Mock
        Assert.Equal("Mock", result.Value.ProviderName);

        // Verify that a warning log was issued (optional)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void GetClient_ReturnsError_WhenNeitherProviderNorMockExists()
    {
        // ARRANGE
        // Situation where no clients are registered (e.g., DI configuration error)
        var clients = new List<IAiClient>(); 
        var factory = new AIProviderFactory(clients, _mockLogger.Object);

        // ACT
        var result = factory.GetClient("OpenAI");

        // ASSERT
        Assert.False(result.Success);
        // Since even "Mock" is missing, it should return a ProviderNotFoundError
        Assert.IsType<ProviderNotFoundError>(result.Error);
    }
}
