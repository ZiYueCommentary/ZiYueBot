using ZiYueBot.Core;

namespace ZiYueBot.Utils;

public static class StringExtension
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
}