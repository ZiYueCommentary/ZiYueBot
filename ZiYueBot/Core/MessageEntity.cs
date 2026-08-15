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

    public string ToString(Context? context)
    {
        IEnumerable<string> raw = this.Select(message => message.ToString(context));
        return string.Join(null, raw);
    }
}

/// <summary>
/// 消息实体类型。
/// </summary>
public enum MessageEntityType
{
    Text
}

/// <summary>
/// 消息实体。一条消息由多个消息实体组成，每个实体代表不同的文字、图片、提及。
/// </summary>
public interface IMessageEntity
{
    public MessageEntityType Type { get; }

    public string ToString(Context? context);
}

public record TextMessageEntity(string Text) : IMessageEntity
{
    public MessageEntityType Type => MessageEntityType.Text;

    public string ToString(Context? context)
    {
        return Text;
    }
}
