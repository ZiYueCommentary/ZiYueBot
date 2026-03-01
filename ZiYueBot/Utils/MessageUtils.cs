using System.Text;
using System.Text.RegularExpressions;
using ZiYueBot.Core;
using ZiYueBot.Discord;

namespace ZiYueBot.Utils;

public static partial class MessageUtils
{
    public static string DatabaseFriendly(this MessageChain arg, IContext context)
    {
        StringBuilder builder = new StringBuilder();
        foreach (IMessageEntity entity in arg)
        {
            switch (entity)
            {
                case TextMessageEntity text:
                    builder.Append(text.Text.FormatDiscordPing(context).SafeArgument());
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

    public static string FormatDiscordPing(this string text, IContext? context)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(text);
        foreach (ValueMatch match in DiscordPingRegex().EnumerateMatches(text))
        {
            ulong userId = ulong.Parse(text.AsSpan().Slice(match.Index + 2, match.Length - 3));
            string userName = context is null
                ? "{ping=" + userId + "}"
                : context.FetchUserName(userId).GetAwaiter().GetResult();
            builder.Replace($"<@{userId}>", $"@{userName}");
        }

        return builder.ToString();
    }

    [GeneratedRegex("<@(\\d+)*>")]
    public static partial Regex DiscordPingRegex();
}