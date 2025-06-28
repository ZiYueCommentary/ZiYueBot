using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class PickStraitbottle : GeneralCommand
{
    public static readonly ILog Logger = LogManager.GetLogger("捞海峡云瓶");

    public override string Id => "捞海峡云瓶";

    public override string Name => "捞海峡云瓶";

    public override string Summary => "捞一个海峡云瓶";

    public override string Description => """
                                          /捞海峡云瓶
                                          扔一个海峡云瓶。由 QQ 扔出的瓶子只能被 Discord 捞起，反之亦然。所有瓶子只能被捞起一次。
                                          频率限制：每次调用间隔 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/straitbottle/pick
                                          """;

    public override Platform SupportedPlatform => Platform.Both;

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（每分钟 1 条）";

        Logger.Info($"调用者：{userName} ({userId})");
        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command = new MySqlCommand(
            "SELECT * FROM straitbottles WHERE picked = false AND fromDiscord = true ORDER BY RAND() LIMIT 1",
            database);
        using MySqlDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return "找不到瓶子！";

        string result = $"""
                         你捞到了 {reader.GetString("username")} 的瓶子！
                         日期：{reader.GetDateTime("created"):yyyy年MM月dd日}

                         {reader.GetString("content")}
                         """;

        using MySqlCommand addViews =
            new MySqlCommand($"UPDATE straitbottles SET picked = true WHERE id = {reader.GetInt32("id")}", database);
        reader.Close();
        addViews.ExecuteNonQuery();

        return result;
    }

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（每分钟 1 条）";

        Logger.Info($"调用者：{userPing} ({userId})");
        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command = new MySqlCommand(
            "SELECT * FROM straitbottles WHERE picked = false AND fromDiscord = false ORDER BY RAND() LIMIT 1",
            database);
        using MySqlDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return "找不到瓶子！";

        string result = $"""
                         你捞到了 {reader.GetString("username")} 的瓶子！
                         日期：{reader.GetDateTime("created"):yyyy年MM月dd日}

                         {reader.GetString("content")}
                         """;

        using MySqlCommand addViews =
            new MySqlCommand($"UPDATE straitbottles SET picked = true WHERE id = {reader.GetInt32("id")}", database);
        reader.Close();
        addViews.ExecuteNonQuery();

        return result;
    }

    public override TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }
}