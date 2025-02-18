using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class PickDriftbottle : IGeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("捞云瓶");

    public string GetCommandId()
    {
        return "捞云瓶";
    }

    public string GetCommandName()
    {
        return "捞云瓶";
    }

    public string GetCommandDescription()
    {
        return """
               /捞云瓶 [id]
               捞一个漂流云瓶。“id”没有用，因为今天是子悦机器的生日。
               频率限制：QQ 群聊每次调用间隔 1 分钟，私聊不限；Discord 不限。
               在线文档：https://docs.ziyuebot.cn/timeline/anniversary/pick-driftbottle.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "捞一个漂流云瓶";
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

    private string Invoke(int id)
    {
        return $"""
                你捞到了 -1 号瓶子！
                来自：我是谁我来干嘛
                日期：2025年2月18日

                祝你新年万事如意，快乐大吉！
                """;
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        return Invoke(0);
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（每分钟 0 条）";
        Logger.Info($"调用者：{userPing} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        return Invoke(0);
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        if (platform == Platform.Discord || eventType == EventType.DirectMessage) return TimeSpan.Zero;
        return TimeSpan.FromMinutes(1);
    }
}