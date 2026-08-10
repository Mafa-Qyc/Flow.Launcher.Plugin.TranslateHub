using System.Net;
using System.Net.Http;
using System.Text.Json;
using Flow.Launcher.Plugin.TranslateHub.Models;
using Flow.Launcher.Plugin.TranslateHub.Providers.Abstractions;
using Flow.Launcher.Plugin.TranslateHub.Services;

namespace Flow.Launcher.Plugin.TranslateHub.Providers.Abstractions;

public abstract class TranslationProviderBase : ITranslationProvider
{
    protected TranslationProviderBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected HttpClient HttpClient { get; }

    public abstract string Id { get; }

    public abstract string DisplayName { get; }

    public abstract ProviderCapabilities Capabilities { get; }

    public virtual bool IsConfigured => true;

    public abstract bool SupportsLanguage(string language);

    public abstract Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken);

    protected static TranslationErrorCode MapExceptionToErrorCode(Exception exception)
    {
        return exception switch
        {
            TranslationException ex => ex.ErrorCode,
            OperationCanceledException => TranslationErrorCode.Timeout,
            HttpRequestException => TranslationErrorCode.Network,
            JsonException => TranslationErrorCode.InvalidResponse,
            _ => TranslationErrorCode.Unknown
        };
    }

    protected async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage message,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.SendAsync(message, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TranslationException(TranslationErrorCode.Timeout, $"{DisplayName} request timed out.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new TranslationException(TranslationErrorCode.Network, $"{DisplayName} network failure.", ex);
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new TranslationException(TranslationErrorCode.RateLimited, $"{DisplayName} rate limited (HTTP 429).");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new TranslationException(
                TranslationErrorCode.ProviderUnavailable,
                $"{DisplayName} returned HTTP {(int)response.StatusCode}.");
        }

        return response;
    }

    protected static async Task<JsonDocument> ReadJsonAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new TranslationException(TranslationErrorCode.InvalidResponse, $"Invalid JSON from provider.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new TranslationException(TranslationErrorCode.Network, $"Failed reading response body.", ex);
        }
    }
}
