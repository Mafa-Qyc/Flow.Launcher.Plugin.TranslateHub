using System.Net.Http;
using Flow.Launcher.Plugin;
using Flow.Launcher.Plugin.TranslateHub.Models;
using Flow.Launcher.Plugin.TranslateHub.Networking;
using Flow.Launcher.Plugin.TranslateHub.Parsing;
using Flow.Launcher.Plugin.TranslateHub.Providers.Bing;
using Flow.Launcher.Plugin.TranslateHub.Providers.Google;
using Flow.Launcher.Plugin.TranslateHub.Providers.Volcengine;
using Flow.Launcher.Plugin.TranslateHub.Services;

namespace Flow.Launcher.Plugin.Translate;

public class Translate : IAsyncPlugin
{
    private const string DefaultSourceLanguage = LanguageCode.Auto;
    private const string DefaultTargetLanguage = LanguageCode.Chinese;
    private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(200);

    private PluginInitContext? _context;
    private TranslationQueryParser? _parser;
    private TranslationService? _service;

    public Task InitAsync(PluginInitContext context)
    {
        _context = context;
        _parser = new TranslationQueryParser();

        var httpClient = HttpClientFactory.GetDefaultClient();
        var registry = new ProviderRegistry();
        registry.Register(new GoogleWebProvider(httpClient), fallbackOrder: 1);
        registry.Register(new BingWebProvider(httpClient), fallbackOrder: 2);
        registry.Register(new VolcengineWebProvider(httpClient), fallbackOrder: 3);

        _service = new TranslationService(registry);

        return Task.CompletedTask;
    }

    public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
        var search = query.Search.Trim();
        var parsed = _parser!.Parse(search, DefaultSourceLanguage, DefaultTargetLanguage);

        if (string.IsNullOrEmpty(parsed.Text))
        {
            return new List<Result>
            {
                new()
                {
                    Title = "TranslateHub",
                    SubTitle = "Usage: tl text · tl >ja text · tl zh>en 你好",
                    IcoPath = "Images\\translate.png",
                    Score = 100
                }
            };
        }

        await Task.Delay(DebounceDelay, token);
        token.ThrowIfCancellationRequested();

        var request = new TranslationRequest(parsed.Text, parsed.SourceLanguage, parsed.TargetLanguage);

        try
        {
            var results = await _service!.TranslateAllAsync(request, token);

            if (results.Count == 0)
            {
                return new List<Result>
                {
                    new()
                    {
                        Title = "No translation available",
                        SubTitle = "All providers failed or timed out",
                        IcoPath = "Images\\translate.png",
                        Score = 1
                    }
                };
            }

            var list = new List<Result>(results.Count);
            var score = results.Count;

            foreach (var r in results)
            {
                list.Add(new Result
                {
                    Title = r.Text,
                    SubTitle = $"{r.ProviderName} · {LanguageResolver.GetName(r.SourceLanguage)} → {LanguageResolver.GetName(r.TargetLanguage)}",
                    IcoPath = "Images\\translate.png",
                    Score = score--,
                    Action = _ =>
                    {
                        _context!.API.CopyToClipboard(r.Text, directCopy: true, showDefaultNotification: false);
                        return true;
                    }
                });
            }

            return list;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return new List<Result>();
        }
        catch (Exception ex)
        {
            return new List<Result>
            {
                new()
                {
                    Title = "Translation failed",
                    SubTitle = ex.Message,
                    IcoPath = "Images\\translate.png",
                    Score = 1
                }
            };
        }
    }
}
