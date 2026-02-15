namespace ZiYueBot.Core;

/// <summary>
/// 命令支持的平台。
/// </summary>
public enum Platform
{
    QQ,
    Discord
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
public abstract class IContext
{
    public abstract Platform Platform { get; }
    public abstract EventType EventType { get; }
    public abstract string UserName { get; }
    public abstract ulong UserId { get; }

    public abstract Task SendMessage(MessageChain messageChain);

    public Task SendMessage(string text) => SendMessage([new TextMessageEntity(text)]);

    public abstract Task<string> FetchUserName(ulong userId);
}