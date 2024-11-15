using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.General;

namespace ZiYueBot.Harmony;

public class ListDriftbottle : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("查看我的瓶子");
    
    public string GetCommandId()
    {
        return "查看我的瓶子";
    }

    public string GetCommandName()
    {
        return "查看我的瓶子";
    }

    public string GetCommandDescription()
    {
        return """
               /查看我的瓶子
               查看你扔出的所有漂流云瓶的相关信息。不包括已删除云瓶。
               频率限制：每次调用间隔 30 分钟。
               在线文档：https://docs.ziyuebot.cn/list-driftbottle.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "查看你所扔出的所有瓶子";
    }

    public string Invoke(EventType type, string userName, ulong userId, string[] args)
    {
        Logger.Info($"调用者：{userName} ({userId})");
        
        using MySqlCommand command = new MySqlCommand(
            $"SELECT * FROM driftbottles WHERE userId = {userId} AND pickable = true",
            ZiYueBot.Instance.Database);
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

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(30);
    }
}