namespace ZiYueBot.Core;

/// <summary>
/// 命令支持的平台。
/// </summary>
public enum Platform
{
    QQ,
    Discord,
    /// <summary>
    /// 管理类命令。与一般的命令不同，它不能在 /help 显示，并且全平台通用。
    /// </summary>
    Management
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
    /// <summary>
    /// 是否拥有群聊/频道的管理权限。在 QQ 为管理员，在 Discord 则为“踢除、批准和拒绝成员”权限。
    /// </summary>
    public abstract bool HasChannelAdmin { get; }

    public abstract Task SendMessage(MessageChain messageChain);

    public Task SendMessage(string text) => SendMessage([new TextMessageEntity(text)]);

    public abstract Task<string> FetchUserName(ulong userId);
}