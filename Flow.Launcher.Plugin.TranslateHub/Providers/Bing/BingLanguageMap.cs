using Flow.Launcher.Plugin.TranslateHub.Models;

namespace Flow.Launcher.Plugin.TranslateHub.Providers.Bing;

public sealed class BingLanguageMap : LanguageMapBase
{
    private static readonly Dictionary<string, string> Map = new()
    {
        [LanguageCode.Chinese] = "zh-Hans",
        [LanguageCode.ChineseTraditional] = "zh-Hant",
        [LanguageCode.Cantonese] = "yue",
        [LanguageCode.English] = "en",
        [LanguageCode.Japanese] = "ja",
        [LanguageCode.Korean] = "ko",
        [LanguageCode.French] = "fr",
        [LanguageCode.German] = "de",
        [LanguageCode.Spanish] = "es",
        [LanguageCode.Russian] = "ru",
        [LanguageCode.Portuguese] = "pt",
        [LanguageCode.Italian] = "it",
        [LanguageCode.Dutch] = "nl",
        [LanguageCode.Turkish] = "tr",
        [LanguageCode.Thai] = "th",
        [LanguageCode.Vietnamese] = "vi",
        [LanguageCode.Indonesian] = "id",
        [LanguageCode.Arabic] = "ar"
    };

    public BingLanguageMap()
        : base(Map, Map)
    {
    }

    public override bool TryMapSource(string normalizedCode, out string providerCode)
    {
        // Bing treats 'auto' as empty source, handled by the provider.
        return base.TryMapSource(normalizedCode, out providerCode);
    }
}
