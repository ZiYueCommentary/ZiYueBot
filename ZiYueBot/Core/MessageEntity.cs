using System.Text.RegularExpressions;

namespace ZiYueBot.Core;

public class MessageChain : List<IMessageEntity>
{
    public static MessageChain operator +(string str, MessageChain chain)
    {
        return [new TextMessageEntity(str), ..chain];
    }

    public static MessageChain operator +(MessageChain chain, string str)
    {
        return [..chain, new TextMessageEntity(str)];
    }

    public bool IsEmpty() => Count == 0;
    public string Flatten() => ToString().Replace("\n", "\\n").Replace("\r", "\\r");
    public override string ToString() => ToString(null);

    public bool IsLiteralString()
    {
        return Find(entity =>
        {
            return entity switch
            {
                ImageMessageEntity => true,
                TextMessageEntity text => TextMessageEntity.DiscordEmotionRegex().IsMatch(text.Text),
                _ => false
            };
        }) is null;
    }

    public string ToString(IContext? context)
    {
        IEnumerable<string> raw = this.Select(message =>
        {
            switch (message.Type)
            {
                case MessageEntityType.Text:
                    return ((TextMessageEntity)message).Text;
                case MessageEntityType.Image:
                    return ((ImageMessageEntity)message).FileName;
                case MessageEntityType.Ping:
                    if (context is null) return " {id=" + ((PingMessageEntity)message).UserId + "} ";
                    return context.FetchUserName(((PingMessageEntity)message).UserId).GetAwaiter().GetResult();
                default:
                    throw new InvalidDataException("unknown message entity type");
            }
        });
        return string.Join(null, raw);
    }

    /// <summary>
    /// 将数据库里的字符串变成可被发送的消息链。所有处理的字符串都只能包含本地图片和远程图片两种数据。
    /// \u2408和\u2409包裹一个本地图片的路径，\uE000和\uE001包裹一个远程图片的路径。
    /// </summary>
    public static MessageChain FromDatabase(string message)
    {
        if (message.Contains('\u2408') || message.Contains('\uE000'))
        {
            MessageChain chain = [];
            int pos = 0;
            for (int i = 0; i < message.Length; i++)
            {
                switch (message[i])
                {
                    case '\u2408': // 本地图片
                    {
                        chain.Add(new TextMessageEntity(message.Substring(pos, i - pos - (pos == 0 ? 0 : 1))));
                        int end = message.IndexOf('\u2409', i + 1);
                        chain.Add(ImageMessageEntity.FromPath(message.Substring(i + 1, end - i - 1)));
                        i = pos = end;
                        continue;
                    }
                    case '\uE000': // 远程图片
                    {
                        chain.Add(new TextMessageEntity(message.Substring(pos, i - pos - (pos == 0 ? 0 : 1))));
                        int end = message.IndexOf('\uE001', i + 1);
                        string path = message.Substring(i + 1, end - i - 1);
                        string filename = path[path.LastIndexOf('/')..];
                        chain.Add(new ImageMessageEntity(path, filename));
                        i = pos = end;
                        continue;
                    }
                }
            }

            if (pos < message.Length - 1)
                chain.Add(new TextMessageEntity(message[(pos + (message[pos + 1] == ' ' ? 2 : 1))..]));
            return chain;
        }

        return [new TextMessageEntity(message)];
    }
}

/// <summary>
/// 消息实体类型。
/// </summary>
public enum MessageEntityType
{
    Text,
    Image,
    Ping
}

/// <summary>
/// 消息实体。一条消息由多个消息实体组成，每个实体代表不同的文字、图片、提及。
/// </summary>
public interface IMessageEntity
{
    public MessageEntityType Type { get; }
}

public partial record TextMessageEntity(string Text) : IMessageEntity
{
    public MessageEntityType Type => MessageEntityType.Text;

    [GeneratedRegex("<:.*:\\d+>")]
    public static partial Regex DiscordEmotionRegex();
}

/// <summary>
/// 图片消息实体。
/// </summary>
/// <param name="Path">图片的路径，可能为本地路径、远程路径，或一个 base64 编码的二进制图片。</param>
public record ImageMessageEntity(string Path, string FileName) : IMessageEntity
{
    public MessageEntityType Type => MessageEntityType.Image;

    /// <summary>
    /// 获取 Discord 用于发送图片的 Stream。
    /// </summary>
    public async Task<Stream> GetStreamAsync()
    {
        if (Path.StartsWith("base64://"))
        {
            string base64Data = Path["base64://".Length..];
            byte[] buffer = Convert.FromBase64String(base64Data);
            return new MemoryStream(buffer);
        }

        Uri uri = new Uri(Path);
        return uri.Scheme.ToLower() switch
        {
            "file" => File.OpenRead(uri.LocalPath),
            "http" or "https" => await new HttpClient().GetStreamAsync(uri),
            _ => throw new NotSupportedException($"Scheme '{uri.Scheme}' is not supported")
        };
    }

    public static ImageMessageEntity FromPath(string path)
    {
        string filename = path[path.LastIndexOf('/')..];
        return new ImageMessageEntity($"file://{System.IO.Path.GetFullPath(path).Replace("\\", "/")}", filename);
    }
}

public record PingMessageEntity(ulong UserId) : IMessageEntity
{
    public MessageEntityType Type => MessageEntityType.Ping;
}