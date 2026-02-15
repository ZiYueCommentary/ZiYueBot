using ZiYueBot.Core;

namespace ZiYueBot.General;

public class ListStargazer : GeneralCommand
{
    public override string Id => "查看星标云瓶";
    public override string Name => "查看星标云瓶";
    public override string Summary => "查看星标云瓶";
    public override string Description => "";

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        return base.QQInvoke(eventType, userName, userId, args);
    }

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        return base.DiscordInvoke(eventType, userPing, userId, args);
    }

    // private string Invoke(string userName, ulong userId)
    // {
    //
    // }
}