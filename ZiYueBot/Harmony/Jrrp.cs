using log4net;
using System.Security.Cryptography;
using System.Text;
using ZiYueBot.Core;

namespace ZiYueBot.Harmony;

/// <summary>
/// 今日人品
/// </summary>
public class Jrrp : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("今日人品");

    private static readonly Dictionary<int, string> Levels = new()
    {
        [0] = "推荐闷头睡大觉。",
        [20] = "也许今天适合摆烂。",
        [40] = "又是平凡的一天。",
        [60] = "太阳当头照，花儿对你笑。",
        [80] = "出门可能捡到 1 块钱。"
    };

    private static readonly Dictionary<int, string> Jackpots = new()
    {
        [0] = "怎，怎么会这样...",
        [42] = "感觉可以参透宇宙的真理。",
        [77] = "要不要去抽一发卡试试呢...？",
        [100] = "买彩票可能会中大奖哦！"
    };

    public string GetCommandDescription()
    {
        return """
               /jrrp
               获取今日人品。人品值范围由 0 到 100。
               在线文档：https://docs.ziyuebot.cn/harmony/jrrp
               """;
    }

    public string GetCommandId()
    {
        return "jrrp";
    }

    public string GetCommandName()
    {
        return "今日人品";
    }

    public string GetCommandShortDescription()
    {
        return "获取今日人品";
    }

    public string Invoke(EventType eventType, string userName, ulong userId, string[] args)
    {
        Logger.Info($"调用者：{userName} ({userId})");

        if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1) // 愚人节！
        {
            return $"{userName} 的今日人品是 {Random.Shared.Next(Int32.MinValue, 0)}。子悦机器不予评价。";
        }

        StringBuilder builder = new StringBuilder();
        builder.Append(userId).Append(DateTime.Now.DayOfYear).Append(42);
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        int luck = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 101;
        string comment = Jackpots.TryGetValue(luck, out string? value)
            ? value
            : Levels.Last(level => level.Key <= luck).Value;

        return $"{userName} 的今日人品是 {luck}。{comment}";
    }
}