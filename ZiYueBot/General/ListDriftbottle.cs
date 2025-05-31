using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class ListDriftbottle : IGeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("查看我的云瓶");

    public string GetCommandId()
    {
        return "查看我的云瓶";
    }

    public string GetCommandName()
    {
        return "查看我的云瓶";
    }

    public string GetCommandDescription()
    {
        return """
               /查看我的云瓶
               查看你扔出的所有漂流云瓶的相关信息。不包括已删除的云瓶。
               频率限制：QQ 群聊每次调用间隔 30 分钟，私聊间隔 10 分钟；Discord 每次调用间隔 10 分钟。
               在线文档：https://docs.ziyuebot.cn/general/driftbottle/list
               """;
    }

    public string GetCommandShortDescription()
    {
        return "查看你所扔出的所有云瓶";
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

    private string Invoke(string userName, ulong userId)
    {
        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command = new MySqlCommand(
            $"SELECT * FROM driftbottles WHERE userid = {userId} AND pickable = true",
            database);
        using MySqlDataReader reader = command.ExecuteReader();
        if (!reader.HasRows) return "没有属于你的瓶子！";
        string result = $"{userName} 的云瓶列表：\n";
        int i = 1;
        while (reader.Read())
        {
            result +=
                $"- 编号：{reader.GetInt32("id")}，创建时间：{reader.GetDateTime("created"):yyyy-MM-dd}，浏览量：{reader.GetInt32("views")}\n";
            i++;
        }

        result += $"共计：{i - 1} 支瓶子";
        return result;
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId))
            return $"频率已达限制（{(eventType == EventType.DirectMessage ? 10 : 30)} 分钟 1 条）";
        Logger.Info($"调用者：{userName} ({userId})");

        return Invoke(userName, userId);
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（10 分钟 1 条）";
        Logger.Info($"调用者：{userPing} ({userId})");

        return Invoke(userPing, userId);
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        if (platform == Platform.Discord || eventType == EventType.DirectMessage) return TimeSpan.FromMinutes(10);
        return TimeSpan.FromMinutes(30);
    }
}