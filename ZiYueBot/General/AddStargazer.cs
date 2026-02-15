using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class AddStargazer : GeneralCommand
{
    public override string Id => "添加星标";
    public override string Name => "添加星标";
    public override string Summary => "添加云瓶星标";
    public override string Description => """
                                          /添加星标 [id]
                                          对云瓶进行星标操作，将其加入用户的星标列表。
                                          频率限制：每次调用间隔 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/stargazer/add
                                          """;

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（1 分钟 1 条）";
        return Stargazers.AddStargazer(userId, userPing, int.Parse(args[1]), false);
    }

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（1 分钟 1 条）";
        return Stargazers.AddStargazer(userId, userName, int.Parse(args[1]), false);
    }

    public override TimeSpan GetRateLimit(Platform? platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }
}