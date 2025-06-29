using System.Collections;
using System.Text.RegularExpressions;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public partial class ThrowStraitbottle : GeneralCommand
{
    public static readonly ILog Logger = LogManager.GetLogger("扔海峡云瓶");

    public override string Id => "扔海峡云瓶";

    public override string Name => "扔海峡云瓶";

    public override string Summary => "扔一个海峡云瓶";

    public override string Description => """
                                          /扔海峡云瓶 [content]
                                          扔一个海峡云瓶。由 QQ 扔出的瓶子只能被 Discord 捞起，反之亦然。所有瓶子只能被捞起一次。
                                          “content”是瓶子的内容，要求不包含表情。
                                          频率限制：每次调用间隔 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/straitbottle/throw
                                          """;

    public override Platform SupportedPlatform => Platform.Both;

    public override string Invoke(Platform platform, EventType eventType, string userName, ulong userId, string[] args)
    {
        if (args.Length < 2) return "参数数量不足。使用“/help 扔海峡云瓶”查看命令用法。";
        string arguments = string.Join(' ', args[1..]);
        if (arguments.Contains('\u2406') || ThrowDriftbottle.PlatformEmojiRegex().IsMatch(arguments)) return "云瓶内容禁止包含表情！";
        if (!RateLimit.TryPassRateLimit(this, platform, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        
        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        UpdateInvokeRecords(userId);

        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command =
            new MySqlCommand(
                "INSERT INTO straitbottles(userid, username, created, content, fromDiscord) VALUE (@userid, @username, now(), @content, false)",
                database);
        command.Parameters.AddWithValue("@userid", userId);
        command.Parameters.AddWithValue("@username", userName);
        command.Parameters.AddWithValue("@content", arguments.DatabaseFriendly());
        command.ExecuteNonQuery();
        return "你的海峡云瓶扔出去了！";
    }

    public override TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }
}