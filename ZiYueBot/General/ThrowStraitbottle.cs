using System.Text.RegularExpressions;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class ThrowStraitbottle : GeneralCommand
{
    public static readonly ILog Logger = LogManager.GetLogger("扔海峡云瓶");

    public override string Id => "扔海峡云瓶";

    public override string Name => "扔海峡云瓶";

    public override string Summary => "扔一个海峡云瓶";

    public override string Description => """
                                          /扔海峡云瓶 [content]
                                          扔一个海峡云瓶。隐玖机器上不可用，请使用子悦机器。
                                          在线文档：https://docs.ziyuebot.cn/general/straitbottle/throw
                                          """;

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        return "海峡云瓶不可用，请使用子悦机器。";
    }

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (ThrowDriftbottle.EmotionRegex().IsMatch(args[1])) return "云瓶内容禁止包含表情！";
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        
        Logger.Info($"调用者：{userPing} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        UpdateInvokeRecords(userId);

        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command =
            new MySqlCommand(
                "INSERT INTO straitbottles(userid, username, created, content, fromDiscord) VALUE (@userid, @username, now(), @content, true)",
                database);
        command.Parameters.AddWithValue("@userid", userId);
        command.Parameters.AddWithValue("@username", Message.MentionedUinAndName[userId]);
        command.Parameters.AddWithValue("@content", args[1].SafeArgument().DatabaseFriendly());
        command.ExecuteNonQuery();
        return "你的海峡云瓶扔出去了！";
    }

    public override TimeSpan GetRateLimit(Platform? platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }
}