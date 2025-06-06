using System.Collections.Concurrent;
using log4net;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Utils;

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

public class StartRevolver : IHarmonyCommand
{
    internal static readonly ConcurrentDictionary<ulong, RevolverRound> Revolvers = [];
    private static readonly ILog Logger = LogManager.GetLogger("开始俄罗斯轮盘");

    public string GetCommandId()
    {
        return "开始俄罗斯轮盘";
    }

    public string GetCommandName()
    {
        return "开始俄罗斯轮盘";
    }

    public string GetCommandDescription()
    {
        return """
               /开始俄罗斯轮盘
               开始一局俄罗斯轮盘。该命令只能在群聊中调用。
               俄罗斯轮盘是一种赌博游戏，相传源于俄罗斯。
               在线文档：https://docs.ziyuebot.cn/harmony/revolver/start
               """;
    }

    public string GetCommandShortDescription()
    {
        return "开始一局俄罗斯轮盘";
    }

    public string Invoke(EventType eventType, string userName, ulong userId, string[] args)
    {
        if (eventType == EventType.DirectMessage) return "俄罗斯轮盘命令只能在群聊中使用！";

        ulong group = ulong.Parse(args[0]);
        if (!RateLimit.TryPassRateLimit(this, eventType, group)) return "频率已达限制（整个群聊内 30 秒 1 条）";

        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        if (Revolvers.TryGetValue(group, out RevolverRound? round) &&
            DateTime.Now - round.StartTime > TimeSpan.FromDays(1))
        {
            Revolvers.Remove(group, out _);
        }

        return Revolvers.TryAdd(group, new RevolverRound()) ? "俄罗斯轮盘开始了，今天轮到谁倒霉呢" : "俄罗斯轮盘已开始";
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromSeconds(30);
    }
}