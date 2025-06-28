using log4net;
using ZiYueBot.Core;

namespace ZiYueBot.Harmony;

public class About : HarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("关于");

    public override string Id => "about";

    public override string Name => "关于";

    public override string Summary => "关于子悦机器";

    public override string Description => """
                                          /about
                                          获取子悦机器的信息。
                                          在线文档：https://docs.ziyuebot.cn/harmony/about
                                          """;

    public override string Invoke(EventType eventType, string userName, ulong userId, string[] args)
    {
        Logger.Info($"调用者：{userName} ({userId})");
        UpdateInvokeRecords(userId);
        return $"""
                子悦机器 (ZiYue Bot) - v{ZiYueBot.Version}
                子悦机器是一个由 子悦解说 开发的，用 C# 编写的 QQ 和 Discord 机器人。
                在线文档：https://docs.ziyuebot.cn/
                使用教程：https://docs.ziyuebot.cn/usage
                用户协议：https://docs.ziyuebot.cn/tos
                开源仓库：https://github.com/ZiYueCommentary/ZiYueBot
                """;
    }
}