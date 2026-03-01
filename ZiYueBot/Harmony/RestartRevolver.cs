using log4net;
using ZiYueBot.Core;
using ZiYueBot.Discord;
using ZiYueBot.QQ;
using ZiYueBot.Utils;

namespace ZiYueBot.Harmony;

public class RestartRevolver : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("重置俄罗斯轮盘");

    public override string Id => "重置俄罗斯轮盘";

    public override string Name => "重置俄罗斯轮盘";

    public override string Summary => "重置俄罗斯轮盘的膛室位置";

    public override string Description => """
                                          /重置俄罗斯轮盘
                                          重新旋转俄罗斯轮盘的膛室，同时不改变子弹所在膛室位置。该命令只能在群聊中调用。
                                          在调用此命令前，必须先调用“开始俄罗斯轮盘”命令。俄罗斯轮盘是一种赌博游戏，相传源于俄罗斯。
                                          在线文档：https://docs.ziyuebot.cn/harmony/revolver/restart
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

        if (!RateLimit.TryPassRateLimit(Id, channelId, TimeSpan.FromSeconds(10)))
        {
            await context.SendMessage("频率已达限制（整个群聊内 10 秒 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})");
        if (!StartRevolver.Revolvers.TryGetValue(channelId, out RevolverRound? round))
        {
            await context.SendMessage("游戏未开始，发送“开始俄罗斯轮盘”来开始");
            return;
        }

        _ = StartRevolver.UpdateRevolverRecords(context.UserId, "restart_count");
        Interlocked.Exchange(ref round.ChamberIndex, Random.Shared.Next(1, RevolverRound.Chambers - 1));
        await context.SendMessage("轮盘已重新旋转");
    }
}