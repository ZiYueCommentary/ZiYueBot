using log4net;

namespace ZiYueBot.General;

public class PicFace : IGeneralCommand
{
    public static readonly ILog Logger = LogManager.GetLogger("表情转图片");
    public static readonly ISet<uint> Users = new HashSet<uint>();

    public Platform GetSupportedPlatform()
    {
        return Platform.QQ;
    }

    public string GetCommandDescription()
    {
        return """
               /表情转图片
               将 QQ 表情包转换为可保存的图片。该命令仅在 QQ 可用。
               在线文档：https://docs.ziyuebot.cn/picface.html
               """;
    }

    public string GetCommandId()
    {
        return "表情转图片";
    }

    public string GetCommandName()
    {
        return "表情转图片";
    }

    public string GetCommandShortDescription()
    {
        return "将表情转换为图片";
    }

    public string QQInvoke(string userName, uint userId, string[] args)
    {
        Logger.Info($"调用者：{userName}（{userId}）");
        Users.Add(userId);
        return "正在等待发送表情包...";
    }

    public string DiscordInvoke(string userPing, ulong userId, string[] args)
    {
        throw new NotImplementedException();
    }
}