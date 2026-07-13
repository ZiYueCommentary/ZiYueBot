using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Discord;
using ZiYueBot.QQ;

namespace ZiYueBot.General;

public class FetchPenalty : Command
{
    public override string Id => "查询记过";
    public override string Name => "查询记过";
    public override string Summary => "查询指定用户的记过数据";

    public override string Description => """
                                          /查询记过 [user]
                                          查询一个用户在全局和本群的记过数据。“user”是一个可选参数，留空默认为自己。
                                          频率限制：每次调用间隔 10 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/fetch-penalty
                                          """;

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (arg.Count > 0 && arg[0].Type != MessageEntityType.Ping)
        {
            await context.SendMessage("参数无效，使用“/help 查询记过”查看命令用法。");
            return;
        }

        ulong userId = arg.Count > 0 ? ((PingMessageEntity)arg[0]).UserId : context.UserId;
        ulong channelId = context.Platform == Platform.QQ
            ? ((QqContext)context).SourceUni
            : ((DiscordContext)context).Socket.GuildId!.Value;
        int penaltyCount = 0;
        string penalty = "";
        await using (MySqlCommand query = new MySqlCommand(
                         $"SELECT * FROM penalty WHERE userid = {userId} AND removed = false",
                         ZiYueBot.Instance.ConnectDatabase()))
        {
            await using MySqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                penalty += $"- 时间：{reader.GetDateTime("created_at"):yyyy年MM月dd日}，原因：{reader.GetString("reason")}";
                penaltyCount++;
            }
        }

        int publicPenaltyCount = 0;
        string publicPenalty = "";
        await using (MySqlCommand query = new MySqlCommand(
                         $"SELECT * FROM penalty_public WHERE userid = {userId} AND channel_id = {channelId} AND removed = false",
                         ZiYueBot.Instance.ConnectDatabase()))
        {
            await using MySqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                publicPenalty += $"- 时间：{reader.GetDateTime("created_at"):yyyy年MM月dd日}，原因：{reader.GetString("reason")}\n";
                publicPenaltyCount++;
            }
        }

        await context.SendMessage($"""
                                   {context.FetchUserName(userId).Result} ({userId}) 的记过数据统计：
                                   {(penaltyCount == 0 ? "该用户没有全局记过记录" : $"该用户共有全局记过 {penaltyCount} 条\n{penalty}")}
                                   {(publicPenaltyCount == 0 ? "该用户没有本群记过记录" : $"该用户共有本群记过 {publicPenaltyCount} 条\n{publicPenalty}")}
                                   """);
    }

    public override TimeSpan GetRateLimit(IContext context)
    {
        return TimeSpan.FromMinutes(10);
    }
}