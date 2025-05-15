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
        // 暂不显示总浏览量。
        using MySqlCommand driftbottlesQuery = new MySqlCommand($"""
                                                                 SELECT (SELECT COUNT(*) FROM driftbottles WHERE userid = {userId})                AS bottle_counts,
                                                                        (SELECT COALESCE(SUM(views), 0) FROM driftbottles WHERE userid = {userId}) AS total_views,
                                                                        (SELECT id FROM driftbottles ORDER BY id DESC LIMIT 1)                     AS last_bottle_id
                                                                 """, ZiYueBot.Instance.ConnectDatabase());
        using MySqlDataReader driftbottlesReader = driftbottlesQuery.ExecuteReader();
        driftbottlesReader.Read();
        string driftbottlesPercent = ((double)driftbottlesReader.GetInt32("bottle_counts") /
            driftbottlesReader.GetInt32("last_bottle_id") * 100).ToString("F4") + "%";
        string driftbottlesStat =
            $"您共扔出了 {driftbottlesReader.GetInt32("bottle_counts")} 支云瓶，占全部云瓶的 {driftbottlesPercent}。";

        // 赞助
        using MySqlCommand sponsorQuery = new MySqlCommand(
            $"SELECT * FROM sponsors WHERE userid = {userId} OR DATE_FORMAT(current_date(), '%m-%d') = '05-03' LIMIT 1",
            ZiYueBot.Instance.ConnectDatabase());
        using MySqlDataReader sponsorReader = sponsorQuery.ExecuteReader();
        string sponsorStat = "";
        if (sponsorReader.Read())
        {
            DateTime sponsorDate = sponsorReader.GetDateTime("date");
            if (DateTime.Today > sponsorDate)
            {
                sponsorStat =
                    $"赞助到期时间：{sponsorDate:yyyy年MM月dd日}（已到期 {(int)(DateTime.Today - sponsorDate).TotalDays} 天）";
            }
            else
            {
                sponsorStat = $"赞助到期时间：{sponsorDate:yyyy年MM月dd日}（{(int)(sponsorDate - DateTime.Today).TotalDays} 天）";
            }
        }
        else
        {
            sponsorStat = "您不是子悦机器的赞助者。";
        }

        // 黑名单
        using MySqlCommand blacklistQuery = new MySqlCommand(
            $"SELECT * FROM blacklists WHERE userid = {userId}",
            ZiYueBot.Instance.ConnectDatabase());
        using MySqlDataReader blacklistReader = blacklistQuery.ExecuteReader();
        string blacklistStat = "您被列入黑名单的命令有：";
        if (blacklistReader.Read())
        {
            do
            {
                blacklistStat += $"/{blacklistReader.GetString("command")}、";
            } while (blacklistReader.Read());

            blacklistStat = blacklistStat[..^1];
        }
        else
        {
            blacklistStat = "您没有被列入黑名单的命令。";
        }

        return $"""
                {userName} 的统计数据
                平台：{(platform == Platform.Discord ? "Discord" : "QQ")}
                ID: {userId}
                {sponsorStat}
                {driftbottlesStat}
                {blacklistStat}
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