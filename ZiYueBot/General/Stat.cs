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
               频率限制：每次调用间隔 5 分钟；赞助者 1 分钟。
               在线文档：https://docs.ziyuebot.cn/general/stat
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
                                                             (SELECT MAX(id) FROM driftbottles)                                         AS last_bottle_id
                                                      """,
                   ZiYueBot.Instance.ConnectDatabase()))
        {
            using MySqlDataReader reader = query.ExecuteReader();
            if (reader.Read())
            {
                int bottleCounts = reader.GetInt32("bottle_counts");
                double percent = (double)bottleCounts / reader.GetInt32("last_bottle_id") * 100;
                driftbottlesStat =
                    $"您共扔出了 {bottleCounts} 支云瓶，占全部云瓶的 {percent:F4}%，总浏览量 {reader.GetInt32("total_views")} 次。";
            }
        }

        // 云瓶增长
        string? driftbottlesIncrementalStat = null;
        using (MySqlCommand query = new MySqlCommand($"""
                                                      SELECT 
                                                          (SELECT COUNT(*) FROM driftbottles WHERE userid = {userId} AND created >= current_date() - INTERVAL 7 DAY) AS your_new_bottles,
                                                          (SELECT COUNT(*) FROM driftbottles WHERE created >= CURRENT_DATE - INTERVAL 7 DAY) AS new_bottles;
                                                      """,
                   ZiYueBot.Instance.ConnectDatabase()))
        {
            using MySqlDataReader reader = query.ExecuteReader();
            if (reader.Read())
            {
                int userNewBottles = reader.GetInt32("your_new_bottles");
                int totalNewBottles = reader.GetInt32("new_bottles");
                double percent = (double)userNewBottles / totalNewBottles * 100;
                driftbottlesIncrementalStat =
                    $"最近七天内增加了 {totalNewBottles} 支云瓶，由你扔出的有 {userNewBottles} 支，占总增长的 {percent:F4}%。";
            }
        }

        // 海峡云瓶
        string? straitbottlesStat = null;
        using (MySqlCommand query = new MySqlCommand($"""
                                                      SELECT (SELECT COUNT(*) FROM straitbottles WHERE userid = {userId})                    AS bottle_counts,
                                                             (SELECT COUNT(*) FROM straitbottles WHERE userid = {userId} AND picked = false) AS unpicked_bottles,
                                                             (SELECT MAX(id) FROM straitbottles)                                             AS last_bottle_id
                                                      """,
                   ZiYueBot.Instance.ConnectDatabase()))
        {
            using MySqlDataReader reader = query.ExecuteReader();
            if (reader.Read())
            {
                int bottleCounts = reader.GetInt32("bottle_counts");
                double percent = (double)bottleCounts / reader.GetInt32("last_bottle_id") * 100;
                straitbottlesStat =
                    $"您共扔出了 {bottleCounts} 支海峡云瓶，占全部海峡云瓶的 {percent:F4}%，其中有 {reader.GetInt32("unpicked_bottles")} 支仍在海峡漂流。";
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
                DateTime sponsorExpiry = reader.GetDateTime("expiry");
                sponsorStatus = $"赞助到期时间：{sponsorExpiry:yyyy年MM月dd日}";
                if (DateTime.Today > sponsorExpiry)
                {
                    sponsorStatus += $"（已到期 {(int)(DateTime.Today - sponsorExpiry).TotalDays} 天）";
                }
                else
                {
                    sponsorStatus += $"（{(int)(sponsorExpiry - DateTime.Today).TotalDays} 天）";
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
                {driftbottlesIncrementalStat ?? "云瓶增长统计失败，请联系子悦。"}
                {straitbottlesStat ?? "海峡云瓶统计失败，请联系子悦。"}
                {blacklists ?? "您没有被列入黑名单的命令。"}
                """;
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（5 分钟 1 条；赞助者每分钟 1 条）";
        Logger.Info($"调用者：{userName} ({userId})");
        return Collect(userName, userId, Platform.QQ);
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（5 分钟 1 条；赞助者每分钟 1 条）";
        Logger.Info($"调用者：{userPing} ({userId})");
        return Collect(userPing, userId, Platform.Discord);
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType, ulong userId)
    {
        using MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command = new MySqlCommand(
            $"SELECT * FROM sponsors WHERE userid = {userId} LIMIT 1",
            connection);
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read() && DateTime.Today <= reader.GetDateTime("expiry"))
        {
            return TimeSpan.FromMinutes(1);
        }

        return TimeSpan.FromMinutes(5);
    }
}