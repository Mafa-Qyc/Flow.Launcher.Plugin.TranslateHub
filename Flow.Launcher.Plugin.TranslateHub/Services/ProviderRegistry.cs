using Flow.Launcher.Plugin.TranslateHub.Providers.Abstractions;

namespace Flow.Launcher.Plugin.TranslateHub.Services;

public sealed class ProviderRegistry
{
    private readonly List<ITranslationProvider> _providers = [];
    private readonly Dictionary<string, int> _orderByProviderId = [];

    public void Register(ITranslationProvider provider, int fallbackOrder)
    {
        _providers.Add(provider);
        _orderByProviderId[provider.Id] = fallbackOrder;
    }

    public IReadOnlyList<ITranslationProvider> GetAllProviders() => _providers;

    public ITranslationProvider? GetProvider(string providerId)
    {
        return _providers.FirstOrDefault(p => p.Id == providerId);
    }

    public IReadOnlyList<ITranslationProvider> GetOrderedProviders()
    {
        return [.. _providers
            .OrderBy(p => _orderByProviderId.GetValueOrDefault(p.Id, int.MaxValue))
            .ThenBy(p => p.Id)];
    }
}
