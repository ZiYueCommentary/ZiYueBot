namespace ZiYueBot.Core;

public struct Message()
{
    public static readonly Dictionary<ulong, string> MentionedUinAndName = [];
    public bool HasForward = false;
    public string Text = "";

    /// <summary>
    /// 解析命令行。该函数会自动跳过开头的斜线，并在空格处分割字符串。
    /// 如果遇到了引号，则该引号到下一个引号之间的内容会被视为一整个部分。
    ///
    /// 例如：
    /// <code>
    /// /example "hello world" ping pong
    /// </code>
    /// 将会返回：
    /// <code>
    /// example、hello world、ping和pong
    /// </code>
    /// </summary>
    public string[] Parse()
    {
        if (Text.Length == 0) return [""];
        IList<string> args = [];
        int pos = Text.First() == '.' ? 1 : 0;
        for (int i = pos; i < Text.Length; i++)
        {
            switch (Text[i])
            {
                case '"':
                {
                    int nextQuote = Text.IndexOf('"', i + 1);
                    if (nextQuote == -1) nextQuote = Text.Length - 1;
                    args.Add(Text.Substring(i + 1, nextQuote - i - 1));
                    i = pos = nextQuote + 2;
                    continue;
                }
                case ' ':
                    args.Add(Text[pos..i]);
                    pos = i + 1;
                    break;
            }
        }

        if (pos < Text.Length) args.Add(Text[pos..]);
        return [.. args];
    }
}