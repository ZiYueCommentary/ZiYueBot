using System.Security.Cryptography.X509Certificates;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class ListStraitbottle : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("海峡云瓶列表");

    public override string Id => "海峡云瓶列表";

    public override string Name => "海峡云瓶列表";

    public override string Summary => "获取海峡云瓶列表";

    public override string Description => """
                                          /海峡云瓶列表
                                          查看当前海峡云瓶生态的数据，包括总瓶子数、可捞起数和扔出数。
                                          频率限制：每次调用间隔 10 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/driftbottle/list
                                          """;

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage("频率已达限制（10 分钟 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})");
        _ = UpdateInvokeRecords(context.UserId);

        await using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        await using MySqlCommand command =
            new MySqlCommand("SELECT * FROM straitbottles WHERE picked = false", database);
        await using MySqlDataReader reader = command.ExecuteReader();
        int i = 0,
            pickable = 0,
            self = 0;
        while (reader.Read())
        {
            if (reader.GetUInt64("userid") == context.UserId) self++;
            if (reader.GetBoolean("fromDiscord") ^ context.Platform == Platform.Discord) pickable++;
            i++;
        }

        if (context.Platform == Platform.QQ)
            await context.SendMessage($"海峡中共有 {i} 支瓶子，其中 {pickable} 支可被 QQ 捞起，{self} 支由你扔出");
        else 
            await context.SendMessage($"海峡中共有 {i} 支瓶子，其中 {pickable} 支可被 Discord 捞起，{self} 支由你扔出");
    }

    public override TimeSpan GetRateLimit(IContext context)
    {
        return TimeSpan.FromMinutes(10);
    }
}