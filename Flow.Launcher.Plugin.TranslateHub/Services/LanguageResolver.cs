namespace Flow.Launcher.Plugin.TranslateHub.Services;

public static class LanguageResolver
{
    private static readonly IReadOnlyDictionary<string, string> Names = new Dictionary<string, string>
    {
        ["auto"] = "Auto",
        ["zh"] = "Chinese",
        ["zh-Hant"] = "Chinese (Traditional)",
        ["yue"] = "Cantonese",
        ["en"] = "English",
        ["ja"] = "Japanese",
        ["ko"] = "Korean",
        ["fr"] = "French",
        ["de"] = "German",
        ["es"] = "Spanish",
        ["ru"] = "Russian",
        ["pt"] = "Portuguese",
        ["it"] = "Italian",
        ["nl"] = "Dutch",
        ["tr"] = "Turkish",
        ["th"] = "Thai",
        ["vi"] = "Vietnamese",
        ["id"] = "Indonesian",
        ["ar"] = "Arabic"
    };

    public static string GetName(string code)
    {
        return Names.TryGetValue(code, out var name) ? name : code;
    }
}
