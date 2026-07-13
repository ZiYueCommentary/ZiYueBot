using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Discord;
using ZiYueBot.QQ;

namespace ZiYueBot.Management;

public class PublicPenalty : Command
{
    public override string Id => "群记过";
    public override string Name => "群记过";
    public override string Summary => "管理命令";

    public override string Description => """
                                          /群记过 [user] [reason]（管理命令）
                                          记录特定用户在群内的违规记录。需要群管理员身份。
                                          在线文档：https://docs.ziyuebot.cn/techical/manangement/public-penalty
                                          """;

    public override Platform[] SupportedPlatform => [Platform.Management];

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (arg.Count < 2)
        {
            await context.SendMessage("参数不足。");
            return;
        }
        if (!context.HasChannelAdmin)
        {
            await context.SendMessage("权限不足。");
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
                $"INSERT INTO penalty_public(userid, channel_id, created_at, created_by, reason) VALUE({targetUserId}, {channelId}, now(), {context.UserId}, @reason)",
                ZiYueBot.Instance.ConnectDatabase());
        command.Parameters.AddWithValue("@reason", reason);
        command.ExecuteNonQuery();
        await context.SendMessage("记录完毕！");
    }
}