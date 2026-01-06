namespace Backend.Dotnet.Application.AIChat.PromptCreator;

public interface IPromptTemplateService
{
    /// <summary>
    /// Render a prompt template with the given model.
    /// </summary>
    /// <param name="templateType">The type of template to use.</param>
    /// <param name="model">The model containing data to render.</param>
    /// <returns>The rendered prompt string.</returns>
    string Render(PromptTemplateType templateType, object model);

    /// <summary>
    /// Get all available template types that have corresponding template files.
    /// </summary>
    IEnumerable<PromptTemplateType> GetAvailableTemplates();
}