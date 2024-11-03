namespace ZiYueBot.QQ;

public static class Parser
{
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
    public static string[] Parse(string line)
    {
        if (line.Length == 0) return [""];
        IList<string> args = [];
        int pos = line.First() == '/' ? 1 : 0;
        for (int i = pos; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                int nextQuote = line.IndexOf('"', i + 1);
                args.Add(line.Substring(i + 1, nextQuote - i - 1));
                i = pos = nextQuote + 2;
                continue;
            }
            if (line[i] == ' ')
            {
                args.Add(line[pos..i]);
                pos = i + 1;
            }
        }
        if (pos < line.Length) args.Add(line[pos..]);
        return [.. args];
    }
}
