using System.Collections.Concurrent;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Discord;
using ZiYueBot.QQ;

namespace ZiYueBot.Harmony;

/// <summary>
/// 一局俄罗斯轮盘游戏。所有位置从 1 开始。
/// </summary>
public class RevolverRound
{
    /// <summary>
    /// 一般而言，左轮有 6 个膛室。
    /// </summary>
    public const int Chambers = 6;

    /// <summary>
    /// 当前膛室位置
    /// </summary>
    public int ChamberIndex = 1;

    /// <summary>
    /// 子弹所在膛室
    /// </summary>
    public readonly int BulletPos = Random.Shared.Next(1, Chambers);

    /// <summary>
    /// 本局开始时间
    /// </summary>
    public readonly DateTime StartTime = DateTime.Now;

    /// <summary>
    /// 返回剩余格数。
    /// </summary>
    public int RestChambers()
    {
        return Chambers - ChamberIndex + 1;
    }
}

public class StartRevolver : Command
{
    internal static readonly ConcurrentDictionary<ulong, RevolverRound> Revolvers = [];
    private static readonly ILog Logger = LogManager.GetLogger("开始俄罗斯轮盘");

    public override string Id => "开始俄罗斯轮盘";

    public override string Name => "开始俄罗斯轮盘";

    public override string Summary => "开始一局俄罗斯轮盘";

    public override string Description => """
                                          /开始俄罗斯轮盘
                                          开始一局俄罗斯轮盘。该命令只能在群聊中调用。
                                          俄罗斯轮盘是一种赌博游戏，相传源于俄罗斯。
                                          在线文档：https://docs.ziyuebot.cn/harmony/revolver/start
                                          """;

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (context.EventType == EventType.DirectMessage)
        {
            await context.SendMessage("俄罗斯轮盘命令只能在群聊中使用！");
            return;
        }

        ulong channelId;
        if (context is DiscordContext discord) channelId = (ulong)discord.Socket.ChannelId!;
        else channelId = ((QqContext)context).SourceUni;

        if (!RateLimit.TryPassRateLimit(this, channelId, TimeSpan.FromSeconds(30)))
        {
            await context.SendMessage("频率已达限制（整个群聊内 30 秒 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})");

        if (Revolvers.TryGetValue(channelId, out RevolverRound? round) &&
            DateTime.Now - round.StartTime > TimeSpan.FromDays(1))
        {
            Revolvers.Remove(channelId, out _);
        }

        if (!Revolvers.TryAdd(channelId, new RevolverRound()))
        {
            await context.SendMessage("俄罗斯轮盘已开始");
            return;
        }

        _ = UpdateRevolverRecords(channelId, "start_count");
        await context.SendMessage("俄罗斯轮盘开始了，今天轮到谁倒霉呢");
    }

    public static async Task UpdateRevolverRecords(ulong userId, string column)
    {
        await using MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase();
        try
        {
            await using MySqlCommand insert =
                new MySqlCommand(
                    $"INSERT INTO revolver (userid, first_invoke, last_invoke) VALUE ({userId}, now(), now())",
                    connection);
            await insert.ExecuteNonQueryAsync();
        }
        catch (Exception)
        {
            // ignored
        }

        await using MySqlCommand update =
            new MySqlCommand(
                $"UPDATE revolver SET last_invoke = now(), {column} = {column} + 1 WHERE userid = {userId}",
                connection);
        await update.ExecuteNonQueryAsync();
    }
}