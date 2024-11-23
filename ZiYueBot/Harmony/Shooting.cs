using log4net;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Utils;

namespace ZiYueBot.Harmony;

public class Shooting : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("俄罗斯轮盘开枪");

    public string GetCommandId()
    {
        return "开枪";
    }

    public string GetCommandName()
    {
        return "俄罗斯轮盘开枪";
    }

    public string GetCommandDescription()
    {
        return """
               /开枪 [user]
               俄罗斯轮盘向某人开枪。“user”为开枪目标，为空时默认向自己开枪。该命令只能在群聊中调用。
               在调用此命令前，必须先调用“开始俄罗斯轮盘”命令。俄罗斯轮盘是一种赌博游戏，相传源于俄罗斯。
               在线文档：https://docs.ziyuebot.cn/shooting.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "向自己或别人开枪（俄罗斯轮盘）";
    }

    public string Invoke(EventType eventType, string userName, ulong userId, string[] args)
    {
        if (eventType == EventType.DirectMessage) return "俄罗斯轮盘命令只能在群聊中使用！";

        ulong group = ulong.Parse(args[0]);
        if (!StartRevolver.Revolvers.TryGetValue(group, out RevolverRound round)) return "游戏未开始，发送“开始俄罗斯轮盘”来开始";
        if (!RateLimit.TryPassRateLimit(this, eventType, userId)) return "频率已达限制（每 3 秒 1 条）";

        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        if (round.ChamberIndex == round.BulletPos)
        {
            StartRevolver.Revolvers.Remove(group);
            return $"砰！枪声响起，{Message.MentionedUinAndName[ulong.Parse(args[1])][1..]} 倒下了";
        }

        StartRevolver.Revolvers[group].ChamberIndex += 1;
        if (StartRevolver.Revolvers[group].RestChambers() == 0) StartRevolver.Revolvers.Remove(group);
        return $"咔哒，无事发生。轮盘中还剩 {StartRevolver.Revolvers[group].RestChambers()} 个膛室未击发。{(round.RestChambers() == 0 ? "本局俄罗斯轮盘结束。" : "")}";
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromSeconds(3);
    }
}