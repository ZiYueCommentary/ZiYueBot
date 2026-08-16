using System.Text.RegularExpressions;
using ZiYueBot.Core;

namespace ZiYueBot.Utils;

public static partial class StringExtension
{
    public static string JsonFriendly(this string str)
    {
        return str.Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    public static string SafeArgument(this string str)
    {
        return str.Replace('\uE000', '[').Replace('\u2408', '[').Replace('\uE001', ']').Replace('\u2409', ']');
    }

    public static string FirstLine(this string str)
    {
        int index = Math.Min(str.IndexOf('\r'), str.IndexOf('\n'));
        return index == -1 ? str : str[..index];
    }

    public static string FormatUrl(this string str)
    {
        IOrderedEnumerable<string> oldUrls = UrlRegex().Matches(str)
            .Select(m => m.Value)
            .Distinct()
            .OrderByDescending(url => url.Length);

        return oldUrls.Aggregate(str, (current, oldUrl) =>
            current.Replace(oldUrl, $"&hyperlink[{oldUrl},1]{oldUrl}")
        );
    }

    [GeneratedRegex(@"\b(?:https?://|www\.)\S+\b")]
    private static partial Regex UrlRegex();
}