using System.Net.Http;
using System.Text;
using System.Text.Json;
using Flow.Launcher.Plugin.TranslateHub.Models;
using Flow.Launcher.Plugin.TranslateHub.Providers.Abstractions;

namespace Flow.Launcher.Plugin.TranslateHub.Providers.Bing;

public sealed class BingWebProvider : TranslationProviderBase
{
    private const string TranslateApi = "https://api-edge.cognitive.microsofttranslator.com/translate";
    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.42";

    private static readonly BingLanguageMap LanguageMap = new();

    private readonly BingTokenProvider _tokenProvider;

    public BingWebProvider(HttpClient httpClient)
        : base(httpClient)
    {
        _tokenProvider = new BingTokenProvider(httpClient);
    }

    public override string Id => "bing";

    public override string DisplayName => "Bing Web";

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

        if (!LanguageMap.TryMapTarget(request.TargetLanguage, out var target))
            throw new TranslationException(
                TranslationErrorCode.UnsupportedLanguage,
                $"Bing Web does not support target language '{request.TargetLanguage}'.");

        var source = request.IsAutoDetect ? "" : request.SourceLanguage;
        if (!request.IsAutoDetect && !LanguageMap.TryMapSource(request.SourceLanguage, out source))
            throw new TranslationException(
                TranslationErrorCode.UnsupportedLanguage,
                $"Bing Web does not support source language '{request.SourceLanguage}'.");

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);

        try
        {
            return await TranslateCoreAsync(request, source, target, token, startedAt, cancellationToken);
        }
        catch (TranslationException ex) when (ex.ErrorCode == TranslationErrorCode.Authentication)
        {
            _tokenProvider.Invalidate();
            var freshToken = await _tokenProvider.GetTokenAsync(cancellationToken);
            return await TranslateCoreAsync(request, source, target, freshToken, startedAt, cancellationToken);
        }
    }

    private async Task<TranslationResult> TranslateCoreAsync(
        TranslationRequest request,
        string source,
        string target,
        string token,
        DateTime startedAt,
        CancellationToken cancellationToken)
    {
        var url = TranslateApi
            + "?from=" + Uri.EscapeDataString(source)
            + "&to=" + Uri.EscapeDataString(target)
            + "&api-version=3.0&includeSentenceLength=true";

        var payload = JsonSerializer.Serialize(new[] { new { Text = request.Text } });

        using var message = new HttpRequestMessage(HttpMethod.Post, url);
        message.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        message.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
        message.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

        using var response = await SendAsync(message, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized
            || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new TranslationException(
                TranslationErrorCode.Authentication,
                $"Bing translation rejected the token (HTTP {(int)response.StatusCode}).");
        }

        using var json = await ReadJsonAsync(response, cancellationToken);

        string? text = null;
        if (json.RootElement.ValueKind == JsonValueKind.Array
            && json.RootElement.GetArrayLength() > 0
            && json.RootElement[0].TryGetProperty("translations", out var translations)
            && translations.ValueKind == JsonValueKind.Array
            && translations.GetArrayLength() > 0
            && translations[0].TryGetProperty("text", out var value)
            && value.ValueKind == JsonValueKind.String)
        {
            text = value.GetString();
        }

        if (string.IsNullOrWhiteSpace(text))
            throw new TranslationException(TranslationErrorCode.InvalidResponse, "Bing Web returned an empty translation.");

        return new TranslationResult
        {
            ProviderId = Id,
            ProviderName = DisplayName,
            Text = text,
            SourceLanguage = request.SourceLanguage,
            TargetLanguage = request.TargetLanguage,
            Duration = DateTime.UtcNow - startedAt,
            IsUnofficialProvider = true
        };
    }
}
