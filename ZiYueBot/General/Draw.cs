using System.Text;
using System.Text.Json.Nodes;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class Draw : GeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("绘画");

    public override string Id => "draw";

    public override string Name => "绘画";

    public override string Summary => "通义万相文生图";

    public override string Description => """
                                          /draw [prompt]
                                          通义万相文生图。隐玖机器上不可用，请使用子悦机器。
                                          在线文档：https://docs.ziyuebot.cn/general/draw
                                          """;

    public override bool Hidden => true;

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        throw new NotSupportedException();
    }

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        throw new NotSupportedException();
    }

    public override TimeSpan GetRateLimit(Platform? platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }
}