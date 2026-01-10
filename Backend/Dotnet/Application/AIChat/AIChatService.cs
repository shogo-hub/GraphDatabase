using Backend.Dotnet.Application.AIChat.PromptCreator;
using Backend.Dotnet.Application.AIChat.AIModelProvider;
using Backend.Dotnet.Common.Errors.Types;
using Backend.Dotnet.Common.Miscellaneous;
using Backend.Dotnet.Controllers.AIChat.Models;
using Microsoft.Extensions.Logging;

/*Main business logic for AIChat system*/ 
namespace Backend.Dotnet.Application.AIChat;

/// <summary>
/// Implementation of <see cref="IAIChatService"/> that orchestrates prompt rendering and AI queries.
/// </summary>
internal sealed class AIChatService : IAIChatService
{
    private readonly AIProviderFactory _prociderFactory;
    private readonly IPromptTemplateService _promptTemplate;
    private readonly ILogger<AIChatService> _logger;

    public AIChatService(
        AIProviderFactory prociderFactory,
        IPromptTemplateService promptTemplate,
        ILogger<AIChatService> logger)
    {
        _prociderFactory = prociderFactory;
        _promptTemplate = promptTemplate;
        _logger = logger;
    }
    ///<summary>
    /// Render into prompt from user question and query to AI
    ///</summary>
    public async Task<TryResult<AIChatResultModel, Error>> QueryAsync(AIChatDomainModel model, CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // Render prompt from template
        var prompt = _promptTemplate.Render(model.TaskType, new
        {
            query = model.Query,
            context = model.Context ?? "No additional context provided."
        });

        _logger.LogInformation(
            "AIChat query started. RequestId={RequestId}, TaskType={TaskType}, Provider={Provider}, QueryLength={QueryLength}",
            requestId, model.TaskType, model.Provider, model.Query.Length);

        // Get the appropriate AI client based on provider
        var clientResult = _prociderFactory.GetClient(model.Provider);
        if (!clientResult.IsSucceeded)
        {
            return TryResult.Fail<Error>(clientResult.Error);
        }

        var aiClient = clientResult.Value;
        var aiResponseResult = await aiClient.QueryAsync(prompt, cancellationToken);

        if (!aiResponseResult.IsSucceeded)
        {
            return TryResult.Fail<Error>(aiResponseResult.Error);
        }

        var aiResponse = aiResponseResult.Value;

        _logger.LogInformation(
            "AIChat query completed. RequestId={RequestId}, Provider={Provider}, ResponseLength={ResponseLength}",
            requestId, model.Provider, aiResponse?.Length ?? 0);

            // Map response to domain model and return success
        var result = AIChatMapper.FromAi(aiResponse ?? string.Empty);
        return TryResult.Succeed(result);
    }
}