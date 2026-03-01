using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class PickStraitbottle : Command
{
    public static readonly ILog Logger = LogManager.GetLogger("捞海峡云瓶");

    public override string Id => "捞海峡云瓶";

    public override string Name => "捞海峡云瓶";

    public override string Summary => "捞一个海峡云瓶";

    public override string Description => """
                                          /捞海峡云瓶
                                          扔一个海峡云瓶。由 QQ 扔出的瓶子只能被 Discord 捞起，反之亦然。所有瓶子只能被捞起一次。
                                          频率限制：每次调用间隔 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/straitbottle/pick
                                          """;

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage("频率已达限制（每分钟 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})");
        _ = UpdateInvokeRecords(context.UserId);

        await using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        await using MySqlCommand command = new MySqlCommand(
            $"SELECT * FROM straitbottles WHERE picked = false AND fromDiscord = {context.Platform == Platform.QQ} ORDER BY RAND() LIMIT 1",
            database);
        await using MySqlDataReader reader = command.ExecuteReader();
        if (!reader.Read())
        {
            await context.SendMessage("找不到瓶子！");
            return;
        }

        string result = $"""
                         你捞到了 {reader.GetString("username")} 的瓶子！
                         日期：{reader.GetDateTime("created"):yyyy年MM月dd日}

                         {reader.GetString("content")}
                         """;

        await using MySqlCommand addViews =
            new MySqlCommand($"UPDATE straitbottles SET picked = true, picked_time = now() WHERE id = {reader.GetInt32("id")}", database);
        await reader.CloseAsync();
        addViews.ExecuteNonQuery();

        await context.SendMessage(MessageChain.FromDatabase(result));
    }

    public override TimeSpan GetRateLimit(IContext context)
    {
        return TimeSpan.FromMinutes(1);
    }
}