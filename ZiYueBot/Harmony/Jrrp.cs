using log4net;
using System.Security.Cryptography;
using System.Text;
using ZiYueBot.Core;

namespace ZiYueBot.Harmony;

public class Jrrp : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("今日人品");

    public override string Id => "jrrp";

    public override string Name => "今日人品";

    public override string Summary => "获取今日人品";

    public override string Description => """
                                          /jrrp
                                          获取今日人品。人品值范围由 0 到 100。
                                          在线文档：https://docs.ziyuebot.cn/harmony/jrrp
                                          """;

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

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        Logger.Info($"调用者：{context.UserName} ({context.UserId})");
        _ =UpdateInvokeRecords(context.UserId);

        if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1) // 愚人节！
        {
            await context.SendMessage($"{context.UserName} 的今日人品是 {Random.Shared.Next(int.MinValue, 0)}。子悦机器不予评价。");
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.Append(context.UserId).Append(DateTime.Today.ToBinary()).Append(42);
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        int luck = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 101;
        string comment = Jackpots.TryGetValue(luck, out string? value)
            ? value
            : Levels.Last(level => level.Key <= luck).Value;

        await context.SendMessage($"{context.UserName} 的今日人品是 {luck}。{comment}");
    }
}