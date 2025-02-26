using log4net;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Utils;

namespace ZiYueBot.Harmony;

public class RestartRevolver : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("重置俄罗斯轮盘");
    
    public string GetCommandId()
    {
        return "重置俄罗斯轮盘";
    }

    public string GetCommandName()
    {
        return "重置俄罗斯轮盘";
    }

    public string GetCommandDescription()
    {
        return """
               /重置俄罗斯轮盘
               重新旋转俄罗斯轮盘的膛室，同时不改变子弹所在膛室位置。该命令只能在群聊中调用。
               在调用此命令前，必须先调用“开始俄罗斯轮盘”命令。俄罗斯轮盘是一种赌博游戏，相传源于俄罗斯。
               在线文档：https://docs.ziyuebot.cn/restart-revolver.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "重置俄罗斯轮盘的膛室位置";
    }

    public string Invoke(EventType eventType, string userName, ulong userId, string[] args)
    {
        if (eventType == EventType.DirectMessage) return "俄罗斯轮盘命令只能在群聊中使用！";

        ulong group = ulong.Parse(args[0]);
        if (!RateLimit.TryPassRateLimit(this, eventType, group)) return "频率已达限制（整个群聊内 10 秒 1 条）";

        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        if (!StartRevolver.Revolvers.TryGetValue(group, out RevolverRound? value)) return "游戏未开始，发送“开始俄罗斯轮盘”来开始";
        Interlocked.Exchange(ref value.ChamberIndex, Random.Shared.Next(1, RevolverRound.Chambers - 1));
        return "轮盘已重新旋转";
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromSeconds(10);
    }
}