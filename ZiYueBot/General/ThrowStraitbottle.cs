using System.Text.RegularExpressions;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class ThrowStraitbottle : Command
{
    public static readonly ILog Logger = LogManager.GetLogger("扔海峡云瓶");

    public override string Id => "扔海峡云瓶";

    public override string Name => "扔海峡云瓶";

    public override string Summary => "扔一个海峡云瓶";

    public override string Description => """
                                          /扔海峡云瓶 [content]
                                          扔一个海峡云瓶。由 QQ 扔出的瓶子只能被 Discord 捞起，反之亦然。所有瓶子只能被捞起一次。
                                          “content”是瓶子的内容，要求不包含表情。
                                          频率限制：每次调用间隔 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/straitbottle/throw
                                          """;

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        await context.SendMessage("暂不可用~");
        return;
        if (arg.IsEmpty())
        {
            await context.SendMessage("参数数量不足。使用“/help 扔海峡云瓶”查看命令用法。");
            return;
        }

        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage("频率已达限制（每分钟 1 条）");
            return;
        }

        if (TextMessageEntity.DiscordEmotionRegex().IsMatch(arg.ToString()))
        {
            await context.SendMessage("云瓶内容禁止包含表情！");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);

        await using MySqlCommand command =
            new MySqlCommand(
                "INSERT INTO straitbottles(userid, username, created, content, fromDiscord) VALUE (@userid, @username, now(), @content, true)",
                ZiYueBot.Instance.ConnectDatabase());
        command.Parameters.AddWithValue("@userid", context.UserId);
        command.Parameters.AddWithValue("@username", context.UserName);
        command.Parameters.AddWithValue("@content", arg.DatabaseFriendly());
        command.ExecuteNonQuery();
        await context.SendMessage("你的海峡云瓶扔出去了！");
    }

    public override TimeSpan GetRateLimit(IContext context)
    {
        return TimeSpan.FromMinutes(1);
    }
}