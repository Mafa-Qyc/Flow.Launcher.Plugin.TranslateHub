namespace Flow.Launcher.Plugin.TranslateHub.Providers.Abstractions;

public interface ILanguageMap
{
    bool TryMapSource(string normalizedCode, out string providerCode);

    bool TryMapTarget(string normalizedCode, out string providerCode);
}
