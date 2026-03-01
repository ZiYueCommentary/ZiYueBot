using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public partial class ThrowDriftbottle : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("扔云瓶");

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

    public override async Task Invoke(IContext context, MessageChain arg)
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

        Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);

        if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1) // 愚人节！
        {
            if (Random.Shared.Next(3) == 1) // 25% 概率瓶子飘回来
            {
                await context.SendMessage("你的瓶子飘回来了，没有扔出去！");
                return;
            }
        }

        if (TextMessageEntity.DiscordEmotionRegex().IsMatch(arg.ToString()))
        {
            await context.SendMessage("云瓶内容禁止包含表情！");
            return;
        }

        bool privileged = Privileged.HasPrivilege(context.UserId, Privilege.BypassDriftbottleQueue);

        await using MySqlCommand command =
            new MySqlCommand(
                $"""
                 INSERT INTO {(privileged ? "driftbottles" : " driftbottles_queue")}(userid, username, created, content) 
                 VALUE (@userid, @username, now(), @content)
                 """,
                ZiYueBot.Instance.ConnectDatabase());
        command.Parameters.AddWithValue("@userid", context.UserId);
        command.Parameters.AddWithValue("@username", context.UserName);
        command.Parameters.AddWithValue("@content", arg.DatabaseFriendly(context));
        command.ExecuteNonQuery();
        if (privileged)
            await context.SendMessage($"[提权] 你的 {command.LastInsertedId} 号云瓶扔出去了！");
        else
            await context.SendMessage($"""
                                       您的云瓶已提交待审，审核编号：{command.LastInsertedId}
                                       审核列表：https://www.ziyuebot.cn/queue.php?id={context.UserId}
                                       """);
    }

    public override TimeSpan GetRateLimit(IContext context)
    {
        return TimeSpan.FromMinutes(1);
    }
}