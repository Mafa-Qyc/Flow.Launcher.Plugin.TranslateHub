using Flow.Launcher.Plugin.TranslateHub.Providers.Abstractions;

namespace Flow.Launcher.Plugin.TranslateHub.Providers;

public abstract class LanguageMapBase : ILanguageMap
{
    protected LanguageMapBase(IReadOnlyDictionary<string, string> sourceMap, IReadOnlyDictionary<string, string> targetMap)
    {
        SourceMap = sourceMap;
        TargetMap = targetMap;
    }

    protected IReadOnlyDictionary<string, string> SourceMap { get; }

    protected IReadOnlyDictionary<string, string> TargetMap { get; }

    public virtual bool TryMapSource(string normalizedCode, out string providerCode)
        => SourceMap.TryGetValue(normalizedCode, out providerCode!);

    public virtual bool TryMapTarget(string normalizedCode, out string providerCode)
        => TargetMap.TryGetValue(normalizedCode, out providerCode!);
}
