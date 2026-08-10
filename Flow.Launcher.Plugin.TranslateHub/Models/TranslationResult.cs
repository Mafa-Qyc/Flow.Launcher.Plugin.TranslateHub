namespace Flow.Launcher.Plugin.TranslateHub.Models;

public sealed record TranslationResult
{
    public required string ProviderId { get; init; }

    public required string ProviderName { get; init; }

    public required string Text { get; init; }

    public required string SourceLanguage { get; init; }

    public required string TargetLanguage { get; init; }

    public string? DetectedSourceLanguage { get; init; }

    public TimeSpan? Duration { get; init; }

    public bool IsUnofficialProvider { get; init; }
}
