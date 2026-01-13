using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Backend.Dotnet.Controllers.Service.AIChat;
using Backend.Dotnet.Application.AIChat;
using Backend.Dotnet.Controllers.Service.AIChat.Models;
using Backend.Dotnet.Common.Miscellaneous;
using Backend.Dotnet.Common.Errors;

namespace Backend.Dotnet.Tests.Unit.Controllers;

public class AIChatControllerTests
{
    private readonly Mock<IAIChatService> _mockService;
    private readonly Mock<ILogger<AIChatController>> _mockLogger;
    private readonly Mock<IProblemDetailsFactory> _mockProblemDetailsFactory;
    private readonly AIChatController _controller;

    public AIChatControllerTests()
    {
        _mockService = new Mock<IAIChatService>();
        _mockLogger = new Mock<ILogger<AIChatController>>();
        _mockProblemDetailsFactory = new Mock<IProblemDetailsFactory>();

        _controller = new AIChatController(
            _mockService.Object,
            _mockLogger.Object,
            _mockProblemDetailsFactory.Object);
    }

    [Fact]
    public async Task PostQuery_ReturnsBadRequest_WhenQueryIsEmpty()
    {
        // ARRANGE
        var request = new AIChatQueryRequest { Query = "", Provider = AiProvider.Mock };

        // Simulate model validation failure
        _controller.ModelState.AddModelError("Query", "Required");
        var result = await _controller.QueryAsync(request, CancellationToken.None);

        // ASSERT
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);

        // Serviceが呼ばれていないことを確認 (無駄な処理防止)
        _mockService.Verify(s => s.QueryAsync(It.IsAny<AIChatDomainModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PostQuery_ReturnsOk_WhenServiceReturnsAnswer()
    {
        // ARRANGE
        var request = new AIChatQueryRequest { Query = "Hello", Provider = AiProvider.Mock };

        // モックの設定: QueryAsyncが呼ばれたら、決まった値を返す
        _mockService
            .Setup(s => s.QueryAsync(It.IsAny<AIChatDomainModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TryResult.Succeed(new AIChatResultModel { Output = "Mock Answer" }));

        // ACT
        var result = await _controller.QueryAsync(request, CancellationToken.None);

        // ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AIChatQueryResponse>(okResult.Value);
        Assert.Equal("Mock Answer", response.Result);
    }
}