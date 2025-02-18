using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class PickStraitbottle : IGeneralCommand
{
    public static readonly ILog Logger = LogManager.GetLogger("捞海峡云瓶");
    
    public string GetCommandId()
    {
        return "捞海峡云瓶";
    }

    public string GetCommandName()
    {
        return "捞海峡云瓶";
    }

    public string GetCommandDescription()
    {
        return """
               /捞海峡云瓶
               捞一个海峡云瓶。怎么都只有一个，因为今天是子悦机器的生日。
               频率限制：每次调用间隔 1 分钟。
               在线文档：https://docs.ziyuebot.cn/timeline/anniversary/pick-straitbottle.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "捞一个海峡云瓶";
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        
        Logger.Info($"调用者：{userName} ({userId})");
        return """
               你捞到了 DeliciousH2O 的瓶子！
               注：《SCP秘密实验室》官方中文翻译员
               日期：2025年2月18日

               若敢来犯，必叫你大败而归！
               """;
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        
        Logger.Info($"调用者：{userPing} ({userId})");
        return """
                你捞到了 DeliciousH2O 的瓶子！
                注：《SCP秘密实验室》官方中文翻译员
                日期：2025年2月18日

                若敢来犯，必叫你大败而归！
                """;
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }
}