using Flow.Launcher.Plugin.TranslateHub.Models;

namespace Flow.Launcher.Plugin.TranslateHub.Providers.Google;

public sealed class GoogleLanguageMap : LanguageMapBase
{
    private static readonly Dictionary<string, string> Map = new()
    {
        [LanguageCode.Chinese] = "zh-CN",
        [LanguageCode.ChineseTraditional] = "zh-TW",
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

    public GoogleLanguageMap()
        : base(Map, Map)
    {
    }
}
