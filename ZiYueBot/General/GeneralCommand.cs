using ZiYueBot.Core;

namespace ZiYueBot.General;

public abstract class GeneralCommand : Command
{
    public abstract Platform SupportedPlatform { get; }
    public abstract string QQInvoke(EventType eventType, string userName, uint userId, string[] args);
    public abstract string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args);
}