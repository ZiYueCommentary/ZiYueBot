using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class Stat : GeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("统计");

    public override string Id => "stat";

    public override string Name => "统计";

    public override string Summary => "统计你的账号数据";

    public override string Description => """
                                          /stat
                                          统计你的账号在子悦机器上的数据。
                                          内容包括：所在平台、账号信息、赞助信息、云瓶统计和黑名单信息。
                                          频率限制：每次调用间隔 5 分钟；赞助者 1 分钟。
                                          完整版统计数据：https://www.ziyuebot.cn/stat.html
                                          在线文档：https://docs.ziyuebot.cn/general/stat
                                          """;

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
                    $"最近七天内增加了 {totalNewBottles} 支云瓶，由您扔出的有 {userNewBottles} 支，占总增长的 {percent:F4}%。";
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

        // 俄罗斯轮盘
        string? revolverStat = null;
        using (MySqlCommand query = new MySqlCommand($"SELECT * FROM revolver WHERE userid = {userId}",
                   ZiYueBot.Instance.ConnectDatabase()))
        {
            using MySqlDataReader reader = query.ExecuteReader();
            if (reader.Read())
            {
                double shootingOtherCount = reader.GetInt32("shooting_other_count");
                double shootingOtherDeath = reader.GetInt32("shooting_other_death");
                double shootingSelfCount = reader.GetInt32("shooting_self_count");
                double shootingSelfDeath = reader.GetInt32("shooting_self_death");
                if (shootingOtherCount + shootingSelfCount > 0)
                {
                    revolverStat =
                        $"您在 {reader.GetDateTime("first_invoke"):yyyy年MM月dd日} 第一次调用俄罗斯轮盘命令，开始过 {reader.GetInt32("start_count")} 局轮盘，转轮 {reader.GetInt32("rotating_count")} 次，重置 {reader.GetInt32("restart_count")} 次。";
                    revolverStat +=
                        $"您向别人开过 {shootingOtherCount} 枪，其中打死过 {shootingOtherDeath} 次。您向自己开过 {shootingSelfCount} 次枪，其中打死过 {shootingSelfDeath} 次。总射击准度 {((shootingOtherDeath + shootingSelfDeath) / (shootingOtherCount + shootingSelfCount) * 100):F4}%。";
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
                {revolverStat ?? "您没有调用过俄罗斯轮盘命令。"}
                {blacklists ?? "您没有被列入黑名单的命令。"}
                
                完整版统计数据另见：https://www.ziyuebot.cn/stat.html?id={userId}
                """;
    }

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（5 分钟 1 条；赞助者每分钟 1 条）";

        Logger.Info($"调用者：{userName} ({userId})");
        UpdateInvokeRecords(userId);
        return Collect(userName, userId, Platform.QQ);
    }

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId))
            return "频率已达限制（5 分钟 1 条；赞助者每分钟 1 条）";

        Logger.Info($"调用者：{userPing} ({userId})");
        UpdateInvokeRecords(userId);
        return Collect(userPing, userId, Platform.Discord);
    }

    public override TimeSpan GetRateLimit(Platform? platform, EventType eventType, ulong userId)
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