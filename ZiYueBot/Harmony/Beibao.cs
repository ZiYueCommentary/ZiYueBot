using log4net;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Utils;

namespace ZiYueBot.Harmony;

public class Beibao : HarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("悲报");

    public override string Id => "beibao";

    public override string Name => "悲报";

    public override string Summary => "生成一张悲报";

    public override string Description => """
                                          /beibao [content]
                                          生成一张悲报。“content”是悲报的内容，必须为纯文字。
                                          频率限制：每次调用间隔 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/harmony/beibao
                                          """;

    public override string Invoke(EventType eventType, string userName, ulong userId, string[] args)
    {
        if (args.Length < 2) return "参数数量不足。使用“/help beibao”查看命令用法。";
        if (!MessageUtils.IsSimpleMessage(string.Join(' ', args))) return "请输入纯文字参数。";
        if (!RateLimit.TryPassRateLimit(this, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        
        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        UpdateInvokeRecords(userId);
        return "";
    }

    public override TimeSpan GetRateLimit(Platform? platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }
}