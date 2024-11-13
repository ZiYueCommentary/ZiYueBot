namespace ZiYueBot.Core;

public struct Message()
{
    public static readonly Dictionary<ulong, string> MentionedUinAndName = [];
    public bool HasForward;
    public string Text;
}