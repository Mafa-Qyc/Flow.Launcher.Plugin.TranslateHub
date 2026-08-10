using System.Net.Http;
using System.Text;
using Flow.Launcher.Plugin.TranslateHub.Models;

namespace Flow.Launcher.Plugin.TranslateHub.Providers.Bing;

public sealed class BingTokenProvider
{
    private const string TokenApi = "https://edge.microsoft.com/translate/auth";

    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(9);

    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private string? _cachedToken;
    private DateTime _expiresAtUtc = DateTime.MinValue;

    public BingTokenProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _expiresAtUtc)
            return _cachedToken;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _expiresAtUtc)
                return _cachedToken;

            var message = new HttpRequestMessage(HttpMethod.Get, TokenApi);
            message.Headers.TryAddWithoutValidation(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.42");

            using var response = await _httpClient.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new TranslationException(
                    TranslationErrorCode.Authentication,
                    $"Bing token request failed with HTTP {(int)response.StatusCode}.");

            var token = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
            if (string.IsNullOrEmpty(token))
                throw new TranslationException(TranslationErrorCode.InvalidResponse, "Bing returned an empty token.");

            _cachedToken = token;
            _expiresAtUtc = DateTime.UtcNow + TokenLifetime;
            return token;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TranslationException(TranslationErrorCode.Timeout, "Bing token request timed out.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (TranslationException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new TranslationException(TranslationErrorCode.Network, "Bing token request failed.", ex);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Invalidate()
    {
        _cachedToken = null;
        _expiresAtUtc = DateTime.MinValue;
    }
}
