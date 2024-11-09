using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using ZiYueBot.Core;

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
            switch (line[i])
            {
                case '"':
                {
                    int nextQuote = line.IndexOf('"', i + 1);
                    args.Add(line.Substring(i + 1, nextQuote - i - 1));
                    i = pos = nextQuote + 2;
                    continue;
                }
                case ' ':
                    args.Add(line[pos..i]);
                    pos = i + 1;
                    break;
            }
        }
        if (pos < line.Length) args.Add(line[pos..]);
        return [.. args];
    }

    /// <summary>
    /// 扁平化 QQ 消息，以便于传输给各命令。
    /// </summary>
    public static string FlattenMessage(BotContext context, MessageChain chain, bool ignoreForward = false)
    {
        string result = "";
        string forwardMessage = ""; // 被引用的消息内容
        bool hasForwardMessage = false;
        bool wasMention = false; // 如果上一个消息是提及，则删除一个空格，以便把前后消息看成一个整体。
        for (int i = 0; i < chain.Count; i++)
        {
            // 除纯文本外，其他类型的特殊消息将被控制字符包裹，以便于发送时层级化。
            switch (chain[i])
            {
                case ForwardEntity forward:
                    if (ignoreForward)
                    {
                        result = "";
                        continue;
                    }

                    forwardMessage = FlattenMessage(context,
                        context.GetGroupMessage((uint)chain.GroupUin, forward.Sequence, forward.Sequence)
                            .GetAwaiter().GetResult().First());
                    result = "";
                    hasForwardMessage = true;
                    wasMention = false;
                    break;
                case TextEntity text:
                    result += text.Text[(wasMention && text.Text.StartsWith(' ') ? 1 : 0) ..];
                    wasMention = false;
                    break;
                case ImageEntity image:
                    result += $"\u2402{image.ImageUrl}\u2403";
                    wasMention = false;
                    break;
                case MentionEntity mention:
                    if (ignoreForward && i == 0) continue;
                    result += $"\u2404{mention.Uin}\u2405";
                    wasMention = true;
                    break;
                case FaceEntity face:
                    result += $"\u2406{face.FaceId}\u2407";
                    wasMention = false;
                    break;
            }
        }

        if (!hasForwardMessage) return result;

        return result.Contains(' ')
            ? result.Insert(result.IndexOf(' '), $" \"{forwardMessage}\" ")
            : $"{result} \"{forwardMessage}\"";
    }

    public static MessageBuilder HierarchizeMessage(uint groupUin, string message)
    {
        bool simpleMessage = true;
        MessageBuilder builder = MessageBuilder.Group(groupUin);
        int pos = 0;
        for (int i = 0; i < message.Length; i++)
        {
            switch (message[i])
            {
                case '\u2402': // 图片
                {
                    builder.Text(message.Substring(pos, i - pos - (pos == 0 ? 0 : 1)));
                    int end = message.IndexOf('\u2403', i + 1);
                    builder.Image(WebUtils.DownloadFile(message.Substring(i + 1, end - i - 1)));
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
                case '\u2404': // 提及
                {
                    builder.Text(message.Substring(pos, i - pos - (pos == 0 ? 0 : 1)));
                    int end = message.IndexOf('\u2405', i + 1);
                    builder.Mention(uint.Parse(message.Substring(i + 1, end - i - 1))).Text(" "); // 提及后面必须加空格，否则会显示出错。
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
                case '\u2406': // 表情
                {
                    builder.Text(message.Substring(pos, i - pos - (pos == 0 ? 0 : 1)));
                    int end = message.IndexOf('\u2407', i + 1);
                    builder.Face(ushort.Parse(message.Substring(i + 1, end - i - 1)));
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
            }
        }

        if (simpleMessage) return builder.Text(message);

        if (pos < message.Length - 1) builder.Text(message[(pos + (message[pos + 1] == ' ' ? 2 : 1))..]);
        return builder;
    }

    public static bool IsSimpleMessage(string flatten)
    {
        return !(flatten.Contains('\u2402') || flatten.Contains('\u2404') || flatten.Contains('\u2406') || (flatten.Contains("<:") && flatten.Contains('>') || (flatten.Contains("<@") && flatten.Contains('>'))));
    }
}
