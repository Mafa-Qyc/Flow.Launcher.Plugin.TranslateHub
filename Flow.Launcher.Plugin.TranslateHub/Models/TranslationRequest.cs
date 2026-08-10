namespace Flow.Launcher.Plugin.TranslateHub.Models;

public sealed record TranslationRequest(
    string Text,
    string SourceLanguage,
    string TargetLanguage)
{
    public bool IsAutoDetect => string.IsNullOrEmpty(SourceLanguage) || SourceLanguage == LanguageCode.Auto;
}
