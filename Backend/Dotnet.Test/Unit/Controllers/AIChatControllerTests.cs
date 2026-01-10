using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Backend.Dotnet.Controllers;     // 実際のnamespaceに合わせて調整してください
using Backend.Dotnet.Services.AIChat; // 実際のnamespaceに合わせて調整してください
using Backend.Dotnet.Models.AIChat;   // 実際のnamespaceに合わせて調整してください

namespace Backend.Dotnet.Tests.Unit.Controllers;

public class AIChatControllerTests
{
    private readonly Mock<IAIChatService> _mockService;
    private readonly AIChatController _controller;

    public AIChatControllerTests()
    {
        // 1. Serviceのモックを作成
        _mockService = new Mock<IAIChatService>();
        
        // 2. モックを注入してControllerのインスタンス化
        _controller = new AIChatController(_mockService.Object);
    }

    [Fact]
    public async Task PostQuery_ReturnsBadRequest_WhenQueryIsEmpty()
    {
        // ARRANGE
        var request = new AIChatRequest { Query = "", Provider = "Mock" };

        // ACT
        var result = await _controller.PostQuery(request);

        // ASSERT
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        
        // Serviceが呼ばれていないことを確認 (無駄な処理防止)
        _mockService.Verify(s => s.GetAnswerAsync(It.IsAny<AIChatRequest>()), Times.Never);
    }

    [Fact]
    public async Task PostQuery_ReturnsOk_WhenServiceReturnsAnswer()
    {
        // ARRANGE
        var request = new AIChatRequest { Query = "Hello", Provider = "Mock" };
        var expectedResponse = new AIChatResponse { Answer = "Mock Answer" };

        // モックの設定: GetAnswerAsyncが呼ばれたら、決まった値を返す
        _mockService
            .Setup(s => s.GetAnswerAsync(It.IsAny<AIChatRequest>()))
            .ReturnsAsync(expectedResponse);

        // ACT
        var result = await _controller.PostQuery(request);

        // ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AIChatResponse>(okResult.Value);
        Assert.Equal("Mock Answer", response.Answer);
    }
}