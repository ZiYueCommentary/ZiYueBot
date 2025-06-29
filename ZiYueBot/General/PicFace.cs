using log4net;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class PicFace : GeneralCommand
{
    public static readonly ILog Logger = LogManager.GetLogger("表情转图片");
    public static readonly ISet<ulong> Users = new HashSet<ulong>();

    public override string Id => "表情转图片";

    public override string Name => "表情转图片";

    public override string Summary => "将表情转换为图片";

    public override string Description => """
                                          /表情转图片
                                          将 QQ 表情包转换为可保存的图片。该命令仅在 QQ 可用。
                                          在线文档：https://docs.ziyuebot.cn/general/picface
                                          """;

    public override Platform SupportedPlatform => Platform.QQ;

    public override string Invoke(Platform platform, EventType eventType, string userName, ulong userId, string[] args)
    {
        Logger.Info($"调用者：{userName} ({userId})");
        UpdateInvokeRecords(userId);
        
        Users.Add(userId);
        return "正在等待发送表情包...";
    }
}