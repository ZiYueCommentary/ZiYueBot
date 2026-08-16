using System.Text.RegularExpressions;
using ZiYueBot.Core;

namespace ZiYueBot.Utils;

public static partial class StringExtension
{
    public static string FormatUrl(this string str)
    {
        return UrlRegex().Replace(str, match => $"&hyperlink[{match.Value},1]{match.Value}");
    }

    [GeneratedRegex(@"\b(?:https?://|www\.)[^\s\u4e00-\u9fa5,，.。?？!！;；]*")]
    private static partial Regex UrlRegex();
}