using ZiYueBot.Core;

namespace ZiYueBot.General;

public abstract class GeneralCommand : Command
{
    public virtual Platform[] SupportedPlatform => [Platform.Discord, Platform.QQ];

    public virtual string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        throw new NotSupportedException();
    }

    public virtual string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        throw new NotSupportedException();
    }

    public virtual string YunhuInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        throw new NotSupportedException();
    }
}