using Flow.Launcher.Plugin.TranslateHub.Models;

namespace Flow.Launcher.Plugin.TranslateHub.Providers.Abstractions;

public interface ITranslationProvider
{
    string Id { get; }

    string DisplayName { get; }

    ProviderCapabilities Capabilities { get; }

    bool IsConfigured { get; }

    bool SupportsLanguage(string language);

    Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken);
}
