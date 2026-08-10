using System.Diagnostics;
using Flow.Launcher.Plugin.TranslateHub.Models;
using Flow.Launcher.Plugin.TranslateHub.Providers.Abstractions;

namespace Flow.Launcher.Plugin.TranslateHub.Services;

public sealed class TranslationService
{
    private static readonly TimeSpan ProviderTimeout = TimeSpan.FromSeconds(3);

    private readonly ProviderRegistry _registry;

    public TranslationService(ProviderRegistry registry)
    {
        _registry = registry;
    }

    public async Task<IReadOnlyList<TranslationResult>> TranslateAllAsync(
        TranslationRequest request,
        CancellationToken cancellationToken)
    {
        var providers = _registry.GetAllProviders();

        if (providers.Count == 0)
            throw new TranslationException(TranslationErrorCode.NotConfigured, "No translation provider is enabled.");

        var tasks = providers
            .Where(p => p.IsConfigured)
            .Where(p => p.SupportsLanguage(request.SourceLanguage) && p.SupportsLanguage(request.TargetLanguage))
            .Select(provider => TranslateWithTimeoutAsync(provider, request, cancellationToken))
            .ToList();

        var results = await Task.WhenAll(tasks);
        return results.Where(r => r is not null).ToList()!;
    }

    private async Task<TranslationResult?> TranslateWithTimeoutAsync(
        ITranslationProvider provider,
        TranslationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(ProviderTimeout);
            return await provider.TranslateAsync(request, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }
}
