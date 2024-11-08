using log4net;

namespace ZiYueBot.Harmony;

public class About : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("关于");
    
    public string GetCommandId()
    {
        return "about";
    }

    public string GetCommandName()
    {
        return "关于";
    }

    public string GetCommandDescription()
    {
        return """
               /about
               获取子悦机器的信息。
               在线文档：https://docs.ziyuebot.cn/about.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "关于子悦机器";
    }

    public string Invoke(string userName, ulong userId, string[] args)
    {
        Logger.Info($"调用者：{userName}（{userId}），参数：${string.Join(',', args)}");
        return $"""
               子悦机器 (ZiYue Bot) - v{ZiYueBot.Version}
               子悦机器是一个由 子悦解说 开发的，用 C# 编写的 QQ 和 Discord 机器人。
               开源仓库：https://github.com/ZiYueCommentary/ZiYueBot
               在线文档：https://docs.ziyuebot.cn/
               """;
    }
}