namespace Flow.Launcher.Plugin.TranslateHub.Parsing;

public sealed record ParsedTranslationQuery(
    string Text,
    string SourceLanguage,
    string TargetLanguage);
