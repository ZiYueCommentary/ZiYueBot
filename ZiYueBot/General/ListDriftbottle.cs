using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class ListDriftbottle : GeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("查看我的云瓶");

    public override string Id => "查看我的云瓶";

    public override string Name => "查看我的云瓶";

    public override string Summary => "查看你所扔出的所有云瓶";

    public override string Description => """
                                          /查看我的云瓶
                                          查看你扔出的所有漂流云瓶的相关信息。隐玖机器上不可用，请使用子悦机器。
                                          在线文档：https://docs.ziyuebot.cn/general/driftbottle/list
                                          """;

    private string Invoke(string userName, ulong userId)
    {
        using MySqlCommand bottleCountCommand =
            new MySqlCommand($"SELECT COUNT(*) AS bottles FROM driftbottles WHERE userid = {userId} AND pickable = true",
                ZiYueBot.Instance.ConnectDatabase());
        using MySqlDataReader bottleCountReader = bottleCountCommand.ExecuteReader();
        bottleCountReader.Read();
        int bottleCount = bottleCountReader.GetInt32("bottles");
        if (bottleCount == 0) return "没有属于你的瓶子！";
        using MySqlCommand bottlesCommand = new MySqlCommand(
            bottleCount <= 50
                ? $"SELECT * FROM driftbottles WHERE userid = {userId} AND pickable = true"
                : $"SELECT * FROM driftbottles WHERE userid = {userId} AND pickable = true ORDER BY views DESC LIMIT 50",
            ZiYueBot.Instance.ConnectDatabase()
        );
        using MySqlDataReader bottlesReader = bottlesCommand.ExecuteReader();
        string result = $"{userName} 的云瓶列表{(bottleCount <= 50 ? "" : "（按浏览量排序）")}：\n";
        int i = 1;
        while (bottlesReader.Read())
        {
            result +=
                $"- 编号：{bottlesReader.GetInt32("id")}，创建时间：{bottlesReader.GetDateTime("created"):yyyy-MM-dd}，浏览量：{bottlesReader.GetInt32("views")}\n";
            i++;
        }

        result += $"共计：{bottleCount} 支瓶子{(bottleCount <= 50 ? "" : "，仅显示排名前 50 支")}";
        return result;
    }

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId))
            return $"频率已达限制（{(eventType == EventType.DirectMessage ? 10 : 30)} 分钟 1 条）";

        Logger.Info($"调用者：{userName} ({userId})");
        UpdateInvokeRecords(userId);

        return Invoke(userName, userId);
    }

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（10 分钟 1 条）";

        Logger.Info($"调用者：{userPing} ({userId})");
        UpdateInvokeRecords(userId);

        return Invoke(userPing, userId);
    }

    public override TimeSpan GetRateLimit(Platform? platform, EventType eventType)
    {
        if (platform == Platform.Discord || eventType == EventType.DirectMessage) return TimeSpan.FromMinutes(10);
        return TimeSpan.FromMinutes(30);
    }
}