using ZiYueBot.Core;

namespace ZiYueBot.General;

public abstract class GeneralCommand : Command
{
    public virtual Platform[] SupportedPlatform => Enum.GetValues<Platform>();
    public abstract string QQInvoke(EventType eventType, string userName, uint userId, string[] args);
    public abstract string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args);
}