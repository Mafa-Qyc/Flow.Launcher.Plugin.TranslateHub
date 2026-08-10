using System.Net.Http;
using System.Text;
using System.Text.Json;
using Flow.Launcher.Plugin.TranslateHub.Models;
using Flow.Launcher.Plugin.TranslateHub.Providers.Abstractions;

namespace Flow.Launcher.Plugin.TranslateHub.Providers.Google;

public sealed class GoogleWebProvider : TranslationProviderBase
{
    internal const string TranslateApi =
        "https://translate.google.com/translate_a/single?dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t";

    private static readonly GoogleLanguageMap LanguageMap = new();

    public GoogleWebProvider(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public override string Id => "google";

    public override string DisplayName => "Google Web";

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

        if (request.IsAutoDetect)
        {
            if (!LanguageMap.TryMapTarget(request.TargetLanguage, out var targetAuto))
                throw new TranslationException(
                    TranslationErrorCode.UnsupportedLanguage,
                    $"Google Web does not support target language '{request.TargetLanguage}'.");
            return await TranslateCoreAsync(request, "auto", targetAuto, startedAt, cancellationToken);
        }

        if (!LanguageMap.TryMapSource(request.SourceLanguage, out var source))
            throw new TranslationException(
                TranslationErrorCode.UnsupportedLanguage,
                $"Google Web does not support source language '{request.SourceLanguage}'.");
        if (!LanguageMap.TryMapTarget(request.TargetLanguage, out var target))
            throw new TranslationException(
                TranslationErrorCode.UnsupportedLanguage,
                $"Google Web does not support target language '{request.TargetLanguage}'.");

        return await TranslateCoreAsync(request, source, target, startedAt, cancellationToken);
    }

    private async Task<TranslationResult> TranslateCoreAsync(
        TranslationRequest request,
        string source,
        string target,
        DateTime startedAt,
        CancellationToken cancellationToken)
    {
        var url = TranslateApi
            + "&q=" + Uri.EscapeDataString(request.Text)
            + "&sl=" + Uri.EscapeDataString(source)
            + "&tl=" + Uri.EscapeDataString(target)
            + "&hl=" + Uri.EscapeDataString(target)
            + "&client=gtx&ie=UTF-8&oe=UTF-8&otf=1&ssel=0&tsel=0&kc=7";

        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await SendAsync(message, cancellationToken);
        using var json = await ReadJsonAsync(response, cancellationToken);

        if (json.RootElement.ValueKind != JsonValueKind.Array || json.RootElement.GetArrayLength() == 0)
            throw new TranslationException(TranslationErrorCode.InvalidResponse, "Google Web returned an unexpected response shape.");

        var segments = json.RootElement[0];
        if (segments.ValueKind != JsonValueKind.Array)
            throw new TranslationException(TranslationErrorCode.InvalidResponse, "Google Web returned no translation segments.");

        var text = new StringBuilder();
        foreach (var segment in segments.EnumerateArray())
        {
            if (segment.ValueKind == JsonValueKind.Array && segment.GetArrayLength() > 0)
            {
                var translated = segment[0];
                if (translated.ValueKind == JsonValueKind.String)
                    text.Append(translated.GetString());
            }
        }

        if (text.Length == 0)
            throw new TranslationException(TranslationErrorCode.InvalidResponse, "Google Web returned an empty translation.");

        return new TranslationResult
        {
            ProviderId = Id,
            ProviderName = DisplayName,
            Text = text.ToString(),
            SourceLanguage = request.SourceLanguage,
            TargetLanguage = request.TargetLanguage,
            Duration = DateTime.UtcNow - startedAt,
            IsUnofficialProvider = true
        };
    }
}
