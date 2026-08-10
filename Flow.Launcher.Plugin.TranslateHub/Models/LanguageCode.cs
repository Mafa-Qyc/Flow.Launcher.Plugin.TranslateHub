namespace Flow.Launcher.Plugin.TranslateHub.Models;

public static class LanguageCode
{
    public const string Auto = "auto";
    public const string Chinese = "zh";
    public const string ChineseTraditional = "zh-Hant";
    public const string Cantonese = "yue";
    public const string English = "en";
    public const string Japanese = "ja";
    public const string Korean = "ko";
    public const string French = "fr";
    public const string German = "de";
    public const string Spanish = "es";
    public const string Russian = "ru";
    public const string Portuguese = "pt";
    public const string Italian = "it";
    public const string Dutch = "nl";
    public const string Turkish = "tr";
    public const string Thai = "th";
    public const string Vietnamese = "vi";
    public const string Indonesian = "id";
    public const string Arabic = "ar";

    private static readonly HashSet<string> KnownCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        Auto, Chinese, ChineseTraditional, Cantonese, English, Japanese, Korean,
        French, German, Spanish, Russian, Portuguese, Italian, Dutch, Turkish,
        Thai, Vietnamese, Indonesian, Arabic
    };

    public static bool IsKnown(string? code)
    {
        return !string.IsNullOrWhiteSpace(code) && KnownCodes.Contains(code);
    }
}
