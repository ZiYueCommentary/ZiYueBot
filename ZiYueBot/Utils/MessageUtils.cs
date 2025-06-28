namespace ZiYueBot.Utils;

public static class MessageUtils
{
    public static bool IsSimpleMessage(string flatten)
    {
        return !(flatten.Contains('\u2402') || flatten.Contains('\u2404') || flatten.Contains('\u2406') ||
                 (flatten.Contains("<:") && flatten.Contains('>') ||
                  (flatten.Contains("<@") && flatten.Contains('>'))));
    }

    public static string FlattenArguments(string[] args)
    {
        return args.Aggregate("",
            (current, arg) => current + arg.Replace("\n", "\\n").Replace("\r", "\\r") + ",")[..^1];
    }
}