using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class AddStargazer : Command
{
    public override string Id => "添加星标";
    public override string Name => "添加星标";
    public override string Summary => "添加云瓶星标";

    public override string Description => """
                                          /添加星标 [id]
                                          对云瓶进行星标操作，将其加入用户的星标列表。
                                          频率限制：每次调用间隔 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/stargazer/add
                                          """;

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage("频率已达限制（1 分钟 1 条）");
            return;
        }

        // TODO 表情回复和普通调用的限制分开
        Stargazers.AddStargazer(context.UserId, context.UserName, int.Parse(arg.ToString(context)), false);
    }

    public override TimeSpan GetRateLimit(IContext context)
    {
        return TimeSpan.FromMinutes(1);
    }
}