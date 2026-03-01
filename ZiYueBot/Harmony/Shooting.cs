using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Discord;
using ZiYueBot.QQ;

namespace ZiYueBot.Harmony;

public class Shooting : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("俄罗斯轮盘开枪");

    public override string Id => "开枪";

    public override string Name => "俄罗斯轮盘开枪";

    public override string Summary => "向自己或别人开枪（俄罗斯轮盘）";

    public override string Description => """
                                          /开枪 [user]
                                          俄罗斯轮盘向某人开枪。“user”为开枪目标，为空时默认向自己开枪。该命令只能在群聊中调用。
                                          在调用此命令前，必须先调用“开始俄罗斯轮盘”命令。俄罗斯轮盘是一种赌博游戏，相传源于俄罗斯。
                                          在线文档：https://docs.ziyuebot.cn/harmony/revolver/shooting
                                          """;

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (context.EventType == EventType.DirectMessage)
        {
            await context.SendMessage("俄罗斯轮盘命令只能在群聊中使用！");
            return;
        }

        if (arg.IsEmpty()) arg.Add(new PingMessageEntity(context.UserId));

        if (arg[0] is not PingMessageEntity ping)
        {
            await context.SendMessage("参数无效。使用“/help 开枪”查看命令用法。");
            return;
        }

        ulong channelId;
        if (context is DiscordContext discord) channelId = (ulong)discord.Socket.ChannelId!;
        else channelId = ((QqContext)context).SourceUni;

        if (!RateLimit.TryPassRateLimit(Id, channelId, TimeSpan.FromSeconds(3)))
        {
            await context.SendMessage("频率已达限制（整个群聊内 3 秒 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})，目标：{ping.UserId}");
        if (!StartRevolver.Revolvers.TryGetValue(channelId, out RevolverRound? round))
        {
            await context.SendMessage("游戏未开始，发送“开始俄罗斯轮盘”来开始");
            return;
        }

        _ = StartRevolver.UpdateRevolverRecords(context.UserId,
            ping.UserId == context.UserId ? "shooting_self_count" : "shooting_other_count");
        await using MySqlCommand update =
            new MySqlCommand(
                $"UPDATE revolver SET being_shot = being_shot + 1 WHERE userid = {ping.UserId}",
                ZiYueBot.Instance.ConnectDatabase());
        await update.ExecuteNonQueryAsync();

        if (round.ChamberIndex == round.BulletPos || (DateTime.Today.Month == 4 && DateTime.Today.Day == 1))
        {
            StartRevolver.Revolvers.Remove(channelId, out _);

            _ = StartRevolver.UpdateRevolverRecords(context.UserId,
                ping.UserId == context.UserId ? "shooting_self_death" : "shooting_other_death");
            await context.SendMessage($"砰！枪声响起，{await context.FetchUserName(ping.UserId)} 倒下了");
            return;
        }

        Interlocked.Increment(ref round.ChamberIndex);
        if (round.RestChambers() == 0) StartRevolver.Revolvers.Remove(channelId, out _);
        await context.SendMessage($"咔哒，无事发生。轮盘中还剩 {round.RestChambers()} 个膛室未击发。{(round.RestChambers() == 0 ? "本局俄罗斯轮盘结束。" : "")}");
    }
}