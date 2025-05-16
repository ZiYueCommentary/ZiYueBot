using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class Stat : IGeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("统计");

    public string GetCommandId()
    {
        return "stat";
    }

    public string GetCommandName()
    {
        return "统计";
    }

    public string GetCommandDescription()
    {
        return """
               /stat
               统计你的账号在子悦机器上的数据。
               内容包括：所在平台、账号信息、赞助信息、云瓶统计和黑名单信息。
               频率限制：每次调用间隔 5 分钟。
               在线文档：https://docs.ziyuebot.cn/stat.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "统计你的账号数据";
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

    public string Collect(string userName, ulong userId, Platform platform)
    {
        // 云瓶
        string? driftbottlesStat = null;
        using (MySqlCommand query = new MySqlCommand($"""
                                                      SELECT (SELECT COUNT(*) FROM driftbottles WHERE userid = {userId})                AS bottle_counts,
                                                             (SELECT COALESCE(SUM(views), 0) FROM driftbottles WHERE userid = {userId}) AS total_views,
                                                             (SELECT id FROM driftbottles ORDER BY id DESC LIMIT 1)                     AS last_bottle_id
                                                      """,
                   ZiYueBot.Instance.ConnectDatabase()))
        {
            using MySqlDataReader reader = query.ExecuteReader();
            if (reader.Read())
            {
                int bottleCounts = reader.GetInt32("bottle_counts");
                double percent = (double)bottleCounts / reader.GetInt32("last_bottle_id") * 100;
                driftbottlesStat = $"您共扔出了 {bottleCounts} 支云瓶，占全部云瓶的 {percent:F4}%，总浏览量 {reader.GetInt32("total_views")} 次。";
            }
        }
        
        // 海峡云瓶
        string? straitbottlesStat = null;
        using (MySqlCommand query = new MySqlCommand($"""
                                                       SELECT (SELECT COUNT(*) FROM straitbottles WHERE userid = {userId})                    AS bottle_counts,
                                                              (SELECT COUNT(*) FROM straitbottles WHERE userid = {userId} AND picked = false) AS unpicked_bottles,
                                                              (SELECT id FROM straitbottles ORDER BY id DESC LIMIT 1)                         AS last_bottle_id
                                                       """,
                   ZiYueBot.Instance.ConnectDatabase()))
        {
            using MySqlDataReader reader = query.ExecuteReader();
            if (reader.Read())
            {
                int bottleCounts = reader.GetInt32("bottle_counts");
                double percent = (double)bottleCounts / reader.GetInt32("last_bottle_id") * 100;
                straitbottlesStat = $"您共扔出了 {bottleCounts} 支海峡云瓶，占全部海峡云瓶的 {percent:F4}%，其中有 {reader.GetInt32("unpicked_bottles")} 支仍在海峡漂流。";
            }
        }

        // 赞助
        string? sponsorStatus = null;
        using (MySqlCommand query = new MySqlCommand(
                   $"SELECT * FROM sponsors WHERE userid = {userId} LIMIT 1",
                   ZiYueBot.Instance.ConnectDatabase()))
        {
            using MySqlDataReader reader = query.ExecuteReader();
            if (reader.Read())
            {
                DateTime sponsorDate = reader.GetDateTime("date");
                sponsorStatus = $"赞助到期时间：{sponsorDate:yyyy年MM月dd日}";
                if (DateTime.Today > sponsorDate)
                {
                    sponsorStatus += $"（已到期 {(int)(DateTime.Today - sponsorDate).TotalDays} 天）";
                }
                else
                {
                    sponsorStatus += $"（{(int)(sponsorDate - DateTime.Today).TotalDays} 天）";
                }
            }
        }

        // 黑名单
        string? blacklists = null;
        using (MySqlCommand query = new MySqlCommand(
                   $"SELECT * FROM blacklists WHERE userid = {userId}",
                   ZiYueBot.Instance.ConnectDatabase()))
        {
            using MySqlDataReader reader = query.ExecuteReader();
            if (reader.Read())
            {
                blacklists = "您被列入黑名单的命令有：";
                do
                {
                    blacklists += $"/{reader.GetString("command")}、";
                } while (reader.Read());

                blacklists = blacklists[..^1];
            }
        }

        return $"""
                {userName} 的统计数据
                平台：{(platform == Platform.Discord ? "Discord" : "QQ")}
                ID: {userId}
                {sponsorStatus ?? "您不是子悦机器的赞助者。"}
                {driftbottlesStat ?? "云瓶统计失败，请联系子悦。"}
                {straitbottlesStat ?? "海峡云瓶统计失败，请联系子悦。"}
                {blacklists ?? "您没有被列入黑名单的命令。"}
                """;
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（5 分钟 1 条）";
        Logger.Info($"调用者：{userName} ({userId})");
        return Collect(userName, userId, Platform.QQ);
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（5 分钟 1 条）";
        Logger.Info($"调用者：{userPing} ({userId})");
        return Collect(userPing, userId, Platform.Discord);
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(5);
    }
}