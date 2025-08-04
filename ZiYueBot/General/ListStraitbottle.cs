using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class ListStraitbottle : GeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("海峡云瓶列表");

    public override string Id => "海峡云瓶列表";

    public override string Name => "海峡云瓶列表";

    public override string Summary => "获取海峡云瓶列表";

    public override string Description => """
                                          /海峡云瓶列表
                                          查看当前海峡云瓶生态的数据，包括总瓶子数、可捞起数和扔出数。
                                          频率限制：每次调用间隔 10 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/driftbottle/list
                                          """;

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（10 分钟 1 条）";

        Logger.Info($"调用者：{userName} ({userId})");
        UpdateInvokeRecords(userId);
        
        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command = new MySqlCommand(
            "SELECT * FROM straitbottles WHERE picked = false",
            database);
        using MySqlDataReader reader = command.ExecuteReader();
        int i = 0;
        int pickable = 0;
        int self = 0;
        while (reader.Read())
        {
            if (reader.GetUInt64("userid") == userId) self++;
            if (reader.GetBoolean("fromDiscord")) pickable++;
            i++;
        }

        return $"海峡中共有 {i} 支瓶子，其中 {pickable} 支可被 QQ 捞起，{self} 支由你扔出";
    }

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（10 分钟 1 条）";

        Logger.Info($"调用者：{userPing} ({userId})");
        UpdateInvokeRecords(userId);
        
        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command = new MySqlCommand(
            "SELECT * FROM straitbottles WHERE picked = false",
            database);
        using MySqlDataReader reader = command.ExecuteReader();
        int i = 0;
        int pickable = 0;
        int self = 0;
        while (reader.Read())
        {
            if (reader.GetUInt64("userid") == userId) self++;
            if (!reader.GetBoolean("fromDiscord")) pickable++;
            i++;
        }

        return $"海峡中共有 {i} 支瓶子，其中 {pickable} 支可被 Discord 捞起，{self} 支由你扔出";
    }

    public override TimeSpan GetRateLimit(Platform? platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(10);
    }
}