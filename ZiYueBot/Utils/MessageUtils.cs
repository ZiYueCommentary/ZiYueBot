using System.Text;
using ZiYueBot.Core;

namespace ZiYueBot.Utils;

public static class MessageUtils
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
                    builder.Append(text.Text);
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
}