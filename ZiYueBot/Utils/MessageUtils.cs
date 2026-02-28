using System.Text;
using System.Text.RegularExpressions;
using ZiYueBot.Core;
using ZiYueBot.Discord;

namespace ZiYueBot.Utils;

public static partial class MessageUtils
{
    // TODO 懒得查了，先不改写了
    public static bool IsSimpleMessage(string flatten)
    {
        return !(flatten.Contains('\u2402') || flatten.Contains('\u2404') || flatten.Contains('\u2406') ||
                 (flatten.Contains("<:") && flatten.Contains('>')) ||
                 (flatten.Contains("<@") && flatten.Contains('>')));
    }

    public static string DatabaseFriendly(this MessageChain arg, IContext context)
    {
        StringBuilder builder = new StringBuilder();
        foreach (IMessageEntity entity in arg)
        {
            switch (entity)
            {
                case TextMessageEntity text:
                    builder.Append(FormatDiscordPing(context, text.Text));
                    break;
                case PingMessageEntity ping:
                    builder.Append($"@{context.FetchUserName(ping.UserId)}");
                    break;
                case ImageMessageEntity image:
                    builder.Append($"\uE000{WebUtils.UploadToS3(image)}\uE001");
                    break;
            }
        }

        return builder.ToString();
    }

    public static string FormatDiscordPing(IContext context, ReadOnlySpan<char> text)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(text);
        foreach (ValueMatch match in DiscordPingRegex().EnumerateMatches(text))
        {
            ulong userId = ulong.Parse(text.Slice(match.Index + 2, match.Length - 3));
            string userName = context.FetchUserName(userId).GetAwaiter().GetResult();
            builder.Replace($"<@{userId}>", $"@{userName}");
        }

        return builder.ToString();
    }

    [GeneratedRegex("<@(\\d+)*>")]
    public static partial Regex DiscordPingRegex();
}