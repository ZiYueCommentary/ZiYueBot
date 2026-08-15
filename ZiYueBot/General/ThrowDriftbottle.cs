using Microsoft.Data.Sqlite;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class ThrowDriftbottle : Command
{
    // private static readonly ILog Logger = LogManager.GetLogger("扔云瓶");

    public override string Id => "扔云瓶";

    public override string Name => "扔云瓶";

    public override string Summary => "扔一个漂流云瓶";

    public override string Description => """
                                          /扔云瓶 [content]
                                          扔一个漂流云瓶。“content”是瓶子的内容，要求不包含表情。
                                          频率限制：每次调用间隔 1 分钟。
                                          云瓶生态建设条例：https://docs.ziyuebot.cn/tos-driftbottle
                                          在线文档：https://docs.ziyuebot.cn/general/driftbottle/throw
                                          """;

    public override async Task Invoke(Context context, MessageChain arg)
    {
        if (arg.IsEmpty())
        {
            await context.SendMessage("参数数量不足。使用“/help 扔云瓶”查看命令用法。");
            return;
        }

        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage("频率已达限制（每分钟 1 条）");
            return;
        }

        // Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);
        await using SqliteCommand command =
            new SqliteCommand(
                """
                INSERT INTO driftbottles(userid, username, created, content) 
                VALUE (@userid, @username, now(), @content)
                """,
                ZiYueBot.Instance.ConnectDatabase());
        command.Parameters.AddWithValue("@userid", context.UserId);
        command.Parameters.AddWithValue("@username", context.UserName);
        command.Parameters.AddWithValue("@content", arg.ToString(context));
        await context.SendMessage($"你的 {command.ExecuteNonQuery()} 号云瓶扔出去了！");
    }

    public override TimeSpan GetRateLimit(Context context)
    {
        return TimeSpan.FromMinutes(1);
    }
}