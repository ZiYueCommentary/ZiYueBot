using log4net;
using ZiYueBot.Core;

namespace ZiYueBot.Harmony;

public class Beibao : Command
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

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (arg.IsEmpty())
        {
            await context.SendMessage("参数数量不足。使用“/help balogo”查看命令用法。");
            return;
        }

        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage("频率已达限制（每分钟 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);

        await context.SendMessage("机器生成中...");

        await context.SendMessage([
            new ImageMessageEntity(
                $"base64://{Convert.ToBase64String(Xibao.Render(false, arg.ToString(context)))}",
                "beibao.jpg")
        ]);
    }

    public override TimeSpan GetRateLimit(IContext context)
    {
        return TimeSpan.FromMinutes(1);
    }
}