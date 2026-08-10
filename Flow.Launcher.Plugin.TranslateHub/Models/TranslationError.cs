namespace Flow.Launcher.Plugin.TranslateHub.Models;

public enum TranslationErrorCode
{
    Network,
    Timeout,
    RateLimited,
    Authentication,
    UnsupportedLanguage,
    InvalidResponse,
    ProviderUnavailable,
    NotConfigured,
    Unknown
}

public sealed class TranslationException : Exception
{
    public TranslationException(TranslationErrorCode code, string message, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = code;
    }

    public TranslationErrorCode ErrorCode { get; }
}
