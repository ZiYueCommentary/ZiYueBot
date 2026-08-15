namespace ZiYueBot.Core;

/// <summary>
/// 命令支持的平台。
/// </summary>
public enum Platform
{
    CBMR
}

/// <summary>
/// 命令调用来源。
/// </summary>
public enum EventType
{
    GroupMessage,
    DirectMessage
}

/// <summary>
/// 命令调用上下文。
/// </summary>
public abstract class Context
{
    public abstract Platform Platform { get; }
    public abstract EventType EventType { get; }
    public abstract string UserName { get; }
    public abstract string UserId { get; }

    public abstract Task SendMessage(MessageChain messageChain);

    public Task SendMessage(string text) => SendMessage([new TextMessageEntity(text)]);
}