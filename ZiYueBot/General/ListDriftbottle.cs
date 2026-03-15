using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class ListDriftbottle : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("查看我的云瓶");

    public override string Id => "查看我的云瓶";

    public override string Name => "查看我的云瓶";

    public override string Summary => "查看你所扔出的所有云瓶";

    public override string Description => """
                                          /查看我的云瓶
                                          查看你扔出的所有漂流云瓶的相关信息。不包括已删除的云瓶。
                                          频率限制：QQ 群聊每次调用间隔 30 分钟，私聊间隔 10 分钟；Discord 每次调用间隔 10 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/driftbottle/list
                                          """;

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage(
                context.EventType == EventType.DirectMessage || context.Platform == Platform.Discord
                    ? "频率已达限制（10 分钟 1 条）"
                    : "频率已达限制（30 分钟 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})");
        _ = UpdateInvokeRecords(context.UserId);

        await using MySqlCommand bottleCountCommand =
            new MySqlCommand(
                $"SELECT COUNT(*) AS bottles FROM driftbottles WHERE userid = {context.UserId} AND pickable = true",
                ZiYueBot.Instance.ConnectDatabase());
        await using MySqlDataReader bottleCountReader = bottleCountCommand.ExecuteReader();
        bottleCountReader.Read();
        int bottleCount = bottleCountReader.GetInt32("bottles");
        if (bottleCount == 0)
        {
            await context.SendMessage("没有属于你的瓶子！");
            return;
        }
        await using MySqlCommand bottlesCommand = new MySqlCommand(
            $"""
             SELECT d.id,d.created,d.views,IFNULL(s.star_count, 0) AS star_count FROM driftbottles AS d
                      LEFT JOIN (SELECT bottle_id, COUNT(*) AS star_count FROM stargazers WHERE removed = 0 GROUP BY bottle_id) AS s 
                          ON s.bottle_id = d.id WHERE d.userid = {context.UserId} AND pickable = TRUE
             """ + (bottleCount > 50 ? " ORDER BY views DESC LIMIT 50;" : " ORDER BY d.id;"),
            ZiYueBot.Instance.ConnectDatabase()
        );
        await using MySqlDataReader bottlesReader = bottlesCommand.ExecuteReader();
        string result = $"{context.UserName} 的云瓶列表{(bottleCount <= 50 ? "" : "（按浏览量排序）")}：\n";
        while (bottlesReader.Read())
        {
            result +=
                $"- 编号：{bottlesReader.GetInt32("id")}，创建时间：{bottlesReader.GetDateTime("created"):yyyy-MM-dd}，浏览量：{bottlesReader.GetInt32("views")}，星标数：{bottlesReader.GetInt32("star_count")}\n";
        }

        result += $"共计：{bottleCount} 支瓶子{(bottleCount <= 50 ? "" : "，仅显示排名前 50 支")}";
        await context.SendMessage(result);
    }

    public override TimeSpan GetRateLimit(IContext context)
    {
        return context.EventType == EventType.DirectMessage || context.Platform == Platform.Discord
            ? TimeSpan.FromMinutes(10)
            : TimeSpan.FromMinutes(30);
    }
}