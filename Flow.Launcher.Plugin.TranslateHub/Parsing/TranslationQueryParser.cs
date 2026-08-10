using System.Text.RegularExpressions;
using Flow.Launcher.Plugin.TranslateHub.Models;

namespace Flow.Launcher.Plugin.TranslateHub.Parsing;

public sealed class TranslationQueryParser
{
    private static readonly Regex SourceTargetRegex = new(@"^([a-z_]+)>([a-z_]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SourceOnlyRegex = new(@"^([a-z_]+)>$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TargetOnlyRegex = new(@"^>?([a-z_]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ParsedTranslationQuery Parse(string search, string defaultSource, string defaultTarget)
    {
        if (string.IsNullOrWhiteSpace(search))
            return new ParsedTranslationQuery(string.Empty, defaultSource, defaultTarget);

        var trimmed = search.Trim();

        var spaceIndex = trimmed.IndexOf(' ');
        string prefix;
        string rest;

        if (spaceIndex == -1)
        {
            prefix = trimmed;
            rest = string.Empty;
        }
        else
        {
            prefix = trimmed.Substring(0, spaceIndex);
            rest = trimmed.Substring(spaceIndex + 1).Trim();
        }

        if (prefix.Length > 15)
            return new ParsedTranslationQuery(trimmed, defaultSource, defaultTarget);

        var m1 = SourceTargetRegex.Match(prefix);
        if (m1.Success)
        {
            var source = Normalize(m1.Groups[1].Value);
            var target = Normalize(m1.Groups[2].Value);
            if (LanguageCode.IsKnown(source) && LanguageCode.IsKnown(target))
                return new ParsedTranslationQuery(rest, source, target);
        }

        var m2 = SourceOnlyRegex.Match(prefix);
        if (m2.Success)
        {
            var source = Normalize(m2.Groups[1].Value);
            if (LanguageCode.IsKnown(source))
                return new ParsedTranslationQuery(rest, source, defaultTarget);
        }

        var m3 = TargetOnlyRegex.Match(prefix);
        if (m3.Success)
        {
            var target = Normalize(m3.Groups[1].Value);
            if (LanguageCode.IsKnown(target))
                return new ParsedTranslationQuery(rest, defaultSource, target);
        }

        return new ParsedTranslationQuery(trimmed, defaultSource, defaultTarget);
    }

    private static string Normalize(string code)
    {
        return code.Replace('_', '-').ToLowerInvariant();
    }
}
