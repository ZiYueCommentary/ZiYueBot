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

    public static string DatabaseFriendly(this MessageChain arg)
    {
        return String.Empty;
    }
}