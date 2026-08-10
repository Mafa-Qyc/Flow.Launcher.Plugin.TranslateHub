using System.Net.Http;
using System.Text;
using System.Text.Json;
using Flow.Launcher.Plugin.TranslateHub.Models;
using Flow.Launcher.Plugin.TranslateHub.Providers.Abstractions;

namespace Flow.Launcher.Plugin.TranslateHub.Providers.Volcengine;

public sealed class VolcengineWebProvider : TranslationProviderBase
{
    private const string TranslateApi = "https://translate.volcengine.com/crx/translate/v1";

    private static readonly VolcengineLanguageMap LanguageMap = new();

    public VolcengineWebProvider(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public override string Id => "volcengine";

    public override string DisplayName => "Volcengine Web";

    public override ProviderCapabilities Capabilities =>
        ProviderCapabilities.NoApiKey | ProviderCapabilities.UnofficialWebApi | ProviderCapabilities.AutoDetect;

    public override bool IsConfigured => true;

    public override bool SupportsLanguage(string language)
    {
        return language == LanguageCode.Auto
            || LanguageMap.TryMapSource(language, out _)
            || LanguageMap.TryMapTarget(language, out _);
    }

    public override async Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTime.UtcNow;

        var source = request.IsAutoDetect ? "auto" : request.SourceLanguage;
        if (!request.IsAutoDetect && !LanguageMap.TryMapSource(request.SourceLanguage, out source))
            throw new TranslationException(
                TranslationErrorCode.UnsupportedLanguage,
                $"Volcengine Web does not support source language '{request.SourceLanguage}'.");
        if (!LanguageMap.TryMapTarget(request.TargetLanguage, out var target))
            throw new TranslationException(
                TranslationErrorCode.UnsupportedLanguage,
                $"Volcengine Web does not support target language '{request.TargetLanguage}'.");

        var payload = JsonSerializer.Serialize(new
        {
            text = request.Text,
            source_language = source,
            target_language = target
        });

        using var message = new HttpRequestMessage(HttpMethod.Post, TranslateApi);
        message.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        message.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");

        using var response = await SendAsync(message, cancellationToken);
        using var json = await ReadJsonAsync(response, cancellationToken);

        if (!json.RootElement.TryGetProperty("translation", out var translation)
            || translation.ValueKind != JsonValueKind.String
            || string.IsNullOrEmpty(translation.GetString()))
        {
            throw new TranslationException(TranslationErrorCode.InvalidResponse, "Volcengine Web returned an unexpected response shape.");
        }

        string? detected = null;
        if (request.IsAutoDetect
            && json.RootElement.TryGetProperty("detected_language", out var detectedLang)
            && detectedLang.ValueKind == JsonValueKind.String)
        {
            detected = detectedLang.GetString();
        }

        return new TranslationResult
        {
            ProviderId = Id,
            ProviderName = DisplayName,
            Text = translation.GetString()!,
            SourceLanguage = request.SourceLanguage,
            TargetLanguage = request.TargetLanguage,
            DetectedSourceLanguage = detected,
            Duration = DateTime.UtcNow - startedAt,
            IsUnofficialProvider = true
        };
    }
}
