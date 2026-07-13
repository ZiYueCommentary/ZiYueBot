using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Discord;
using ZiYueBot.QQ;

namespace ZiYueBot.Management;

public class Penalty : Command
{
    public override string Id => "记过";
    public override string Name => "记过";
    public override string Summary => "管理命令";

    public override string Description => """
                                          /记过 [user] [reason]（管理命令）
                                          记录特定用户的违规记录。需要 CreatePenalty 特权。
                                          在线文档：https://docs.ziyuebot.cn/techical/manangement/penalty
                                          """;

    public override Platform[] SupportedPlatform => [Platform.Management];

    public override Task Invoke(IContext context, MessageChain arg)
    {
        return Invoke(context, arg);
    }

    public async Task Invoke(IContext context, MessageChain arg, bool sudo = false)
    {
        if (!Privileged.HasPrivilege(context.UserId, Privilege.CreatePenalty))
        {
            // if (context.EventType == EventType.DirectMessage)
            // {
                await context.SendMessage("权限不足。");
                return;
            // }

            // if (context.HasChannelAdmin && !sudo)
            // {
            //     await context.SendMessage("""
            //                               您不是子悦机器的管理员，应该使用“群记过”添加本群范围内的记录。
            //                               “记过”命令用于添加机器全局记录，并且只能记录与机器相关的违规。
            //                               如果您确认要添加全局记录，请在本条调用前添加“sudo ”，确认提权执行。
            //                               """);
            //     return;
            // }
        }
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