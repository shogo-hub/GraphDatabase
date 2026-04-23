using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;


namespace Backend.Dotnet.Application.AIChat.PromptCreator;

internal sealed class FileBasedPromptTemplateService : IPromptTemplateService
{
    private const PromptTemplateType DefaultTemplateType = PromptTemplateType.Explain;
    private readonly string _templatesPath;
    private readonly ILogger<FileBasedPromptTemplateService> _logger;
    private readonly Dictionary<PromptTemplateType, string> _templateCache;

    public FileBasedPromptTemplateService(
        IWebHostEnvironment env,
        ILogger<FileBasedPromptTemplateService> logger)
    {
        _templatesPath = Path.Combine(
            env.ContentRootPath,
            "Application",
            "AIChat",
            "PromptCreator",
            "PromptTemplate");
        _logger = logger;
        _templateCache = new Dictionary<PromptTemplateType, string>();

        // Create templates directory if it doesn't exist
        if (!Directory.Exists(_templatesPath))
        {
            _logger.LogWarning("Templates directory not found, creating: {Path}", _templatesPath);
            Directory.CreateDirectory(_templatesPath);
        }

        // Discover and cache available templates
        DiscoverTemplates();
    }

    /// <summary>
    /// Discover all available template files and cache them.
    /// </summary>
    private void DiscoverTemplates()
    {
        foreach (var templateType in Enum.GetValues<PromptTemplateType>())
        {
            var templatePath = GetTemplatePath(templateType);
            if (File.Exists(templatePath))
            {
                _templateCache[templateType] = templatePath;
                _logger.LogDebug("Discovered template: {TemplateType} at {Path}", templateType, templatePath);
            }
        }

        _logger.LogInformation(
            "Template discovery complete. Found {Count} templates: {Templates}",
            _templateCache.Count,
            string.Join(", ", _templateCache.Keys));
    }

    /// <inheritdoc />
    public IEnumerable<PromptTemplateType> GetAvailableTemplates() => _templateCache.Keys;

    /// <inheritdoc />
    public string Render(PromptTemplateType templateType, object model)
    {
        var resolvedType = ResolveTemplateType(templateType);
        var templatePath = GetTemplatePath(resolvedType);

        if (!File.Exists(templatePath))
        {
            _logger.LogWarning(
                "Template not found: {TemplatePath}, using built-in default",
                templatePath);
            return RenderDefault(model);
        }

        try
        {
            var template = File.ReadAllText(templatePath);
            return RenderTemplate(template, model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read template: {TemplatePath}", templatePath);
            return RenderDefault(model);
        }
    }

    /// <summary>
    /// Resolve the template type, falling back to default if not available.
    /// </summary>
    private PromptTemplateType ResolveTemplateType(PromptTemplateType requested)
    {
        if (_templateCache.ContainsKey(requested))
        {
            return requested;
        }

        _logger.LogWarning(
            "Requested template {Requested} not found, falling back to {Default}",
            requested, DefaultTemplateType);

        return DefaultTemplateType;
    }

    /// <summary>
    /// Get the file path for a template type.
    /// </summary>
    private string GetTemplatePath(PromptTemplateType templateType)
    {
        var fileName = templateType.ToString().ToLowerInvariant();
        return Path.Combine(_templatesPath, $"{fileName}.tpl");
    }

    private static string RenderTemplate(string template, object model)
    {
        // Simple {{key}} replacement - supports nested properties with dot notation
        var type = model.GetType();
        return Regex.Replace(template, @"\{\{(\w+(?:\.\w+)*)\}\}", match =>
        {
            var propPath = match.Groups[1].Value;
            var value = GetPropertyValue(model, propPath);
            return value?.ToString() ?? string.Empty;
        });
    }

    private static object? GetPropertyValue(object obj, string propertyPath)
    {
        var properties = propertyPath.Split('.');
        object? current = obj;

        foreach (var propName in properties)
        {
            if (current == null) return null;

            var type = current.GetType();
            var prop = type.GetProperty(propName);

            if (prop == null) return null;

            current = prop.GetValue(current);
        }

        return current;
    }

    private static string RenderDefault(object model)
    {
        var type = model.GetType();
        var query = type.GetProperty("query")?.GetValue(model)?.ToString() ?? "";
        var context = type.GetProperty("context")?.GetValue(model)?.ToString() ?? "";

        return $@"You are a helpful assistant.
            Context: {context}
            Question: {query}
            Provide a clear and helpful answer.";
    }
}