using log4net;
using ZiYueBot.Core;
using ZiYueBot.Discord;
using ZiYueBot.QQ;

namespace ZiYueBot.Harmony;

public class Rotating : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("俄罗斯轮盘转轮");

    public override string Id => "转轮";

    public override string Name => "俄罗斯轮盘转轮";

    public override string Summary => "俄罗斯轮盘转轮";

    public override string Description => """
                                          /转轮
                                          俄罗斯轮盘转轮。当剩余格数为 0 时，本局俄罗斯轮盘将结束。该命令只能在群聊中调用。
                                          在调用此命令前，必须先调用“开始俄罗斯轮盘”命令。俄罗斯轮盘是一种赌博游戏，相传源于俄罗斯。
                                          在线文档：https://docs.ziyuebot.cn/harmony/revolver/rotating
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

        if (!RateLimit.TryPassRateLimit(this, channelId, TimeSpan.FromSeconds(3)))
        {
            await context.SendMessage("频率已达限制（整个群聊内 3 秒 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})");
        if (!StartRevolver.Revolvers.TryGetValue(channelId, out RevolverRound? round))
        {
            await context.SendMessage("游戏未开始，发送“开始俄罗斯轮盘”来开始");
            return;
        }

        _ = StartRevolver.UpdateRevolverRecords(context.UserId, "rotating_count");
        Interlocked.Increment(ref round.ChamberIndex);
        if (round.ChamberIndex > RevolverRound.Chambers) StartRevolver.Revolvers.Remove(channelId, out _);
        await context.SendMessage(
            $"已转轮，轮盘中还剩 {round.RestChambers()} 个膛室未击发。{(round.RestChambers() == 0 ? "本局俄罗斯轮盘结束。" : "")}");
    }
}