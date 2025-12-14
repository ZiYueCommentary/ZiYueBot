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
                                          统计你的账号在隐玖机器上的数据。
                                          频率限制：每次调用间隔 5 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/stat
                                          """;

    public string Collect(string userName, ulong userId, Platform platform)
    {
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
                ID: {userId}
                {revolverStat ?? "您没有调用过俄罗斯轮盘命令。"}
                {blacklists ?? "您没有被列入黑名单的命令。"}
                
                子悦机器统计数据另见：https://www.ziyuebot.cn/stat.html?id={userId}
                """;
    }

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（5 分钟 1 条）";

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