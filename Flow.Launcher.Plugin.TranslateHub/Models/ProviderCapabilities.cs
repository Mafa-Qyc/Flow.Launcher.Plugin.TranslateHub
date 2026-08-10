namespace Flow.Launcher.Plugin.TranslateHub.Models;

[Flags]
public enum ProviderCapabilities
{
    None = 0,
    AutoDetect = 1 << 0,
    NoApiKey = 1 << 1,
    OfficialApi = 1 << 2,
    UnofficialWebApi = 1 << 3,
    SelfHosted = 1 << 4,
    AiTranslation = 1 << 5
}
