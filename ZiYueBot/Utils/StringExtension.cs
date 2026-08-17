using System.Text.RegularExpressions;

namespace ZiYueBot.Utils;

public static partial class StringExtension
{
    public static string FormatUrl(this string str)
    {
        return UrlRegex().Replace(str, match => $"&hyperlink[{match.Value},1]{match.Value}");
    }

    [GeneratedRegex(@"https?:\/\/[^\s]+")]
    private static partial Regex UrlRegex();
}