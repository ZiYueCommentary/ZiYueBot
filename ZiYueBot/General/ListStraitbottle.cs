using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class ListStraitbottle : IGeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("海峡云瓶列表");

    public string GetCommandId()
    {
        return "海峡云瓶列表";
    }

    public string GetCommandName()
    {
        return "海峡云瓶列表";
    }

    public string GetCommandDescription()
    {
        return "海峡云瓶列表";
    }

    public string GetCommandShortDescription()
    {
        return "获取海峡云瓶列表";
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（10 分钟 1 条）";

        Logger.Info($"调用者：{userName} ({userId})");
        using MySqlCommand command = new MySqlCommand(
            "SELECT * FROM straitbottles WHERE picked = false",
            ZiYueBot.Instance.Database);
        using MySqlDataReader reader = command.ExecuteReader();
        int i = 0;
        int pickable = 0;
        int self = 0;
        while (reader.Read())
        {
            if (reader.GetUInt64("userId") == userId) self++;
            if (reader.GetBoolean("fromDiscord")) pickable++;
            i++;
        }

        return $"海峡中共有 {i} 支瓶子，其中 {pickable} 支可被 QQ 捞起，{self} 支由你扔出";
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（10 分钟 1 条）";

        Logger.Info($"调用者：{userPing} ({userId})");
        using MySqlCommand command = new MySqlCommand(
            "SELECT * FROM straitbottles WHERE picked = false",
            ZiYueBot.Instance.Database);
        using MySqlDataReader reader = command.ExecuteReader();
        int i = 0;
        int pickable = 0;
        int self = 0;
        while (reader.Read())
        {
            if (reader.GetUInt64("userId") == userId) self++;
            if (!reader.GetBoolean("fromDiscord")) pickable++;
            i++;
        }

        return $"海峡中共有 {i} 支瓶子，其中 {pickable} 支可被 Discord 捞起，{self} 支由你扔出";
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(10);
    }
}