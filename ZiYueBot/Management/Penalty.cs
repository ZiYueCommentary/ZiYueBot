using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Discord;
using ZiYueBot.QQ;

namespace ZiYueBot.Management;

public class Penalty : PrivilegeCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("记过");

    public override string Id => "记过";
    public override string Name => "记过";
    public override string Summary => "管理命令";

    public override string Description => """
                                          /记过 [user] [reason]（管理命令）
                                          记录特定用户的违规记录。需要 CreatePenalty 特权。
                                          在线文档：https://docs.ziyuebot.cn/techical/manangement/penalty
                                          """;

    public override Privilege[] ExpectingPrivileges => [Privilege.CreatePenalty];

    public override async Task PrivilegedInvoke(Context context, MessageChain arg)
    {
        if (arg.Count < 2)
        {
            await context.SendMessage("参数不足。");
            return;
        }

        if (arg[0] is not PingMessageEntity ping)
        {
            await context.SendMessage("参数无效，请检查第一个参数。");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);

        ulong targetUserId = ping.UserId;
        ulong channelId = context.Platform == Platform.QQ
            ? ((QqContext)context).SourceUni
            : ((DiscordContext)context).Socket.GuildId!.Value;
        arg.RemoveAt(0);
        string reason = arg.ToString(context).Trim();
        await using MySqlCommand command =
            new MySqlCommand(
                $"INSERT INTO penalty(userid, channel_id, created_at, created_by, community, reason) VALUE({targetUserId}, {channelId}, now(), {context.UserId}, false, @reason)",
                ZiYueBot.Instance.ConnectDatabase());
        command.Parameters.AddWithValue("@reason", reason);
        command.ExecuteNonQuery();
        await context.SendMessage("记录完毕！");
    }
}