using System.Text.RegularExpressions;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public partial class ThrowDriftbottle : GeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("扔云瓶");

    public override string Id => "扔云瓶";

    public override string Name => "扔云瓶";

    public override string Summary => "扔一个漂流云瓶";

    public override string Description => """
                                          /扔云瓶 [content]
                                          扔一个漂流云瓶。“content”是瓶子的内容，要求不包含表情。
                                          频率限制：每次调用间隔 1 分钟。
                                          云瓶生态建设条例：https://docs.ziyuebot.cn/tos-driftbottle
                                          在线文档：https://docs.ziyuebot.cn/general/driftbottle/throw
                                          """;

    private string Invoke(string userName, ulong userId, string content)
    {
        if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1) // 愚人节！
        {
            if (Random.Shared.Next(3) == 1) // 25% 概率瓶子飘回来
            {
                return "你的瓶子飘回来了，没有扔出去！";
            }
        }

        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command =
            new MySqlCommand(
                "INSERT INTO driftbottles_queue(userid, username, created, content) VALUE (@userid, @username, now(), @content)",
                database);
        command.Parameters.AddWithValue("@userid", userId);
        command.Parameters.AddWithValue("@username", userName);
        command.Parameters.AddWithValue("@content", content.DatabaseFriendly());
        command.ExecuteNonQuery();
        return $"您的云瓶已提交待审，审核编号：{command.LastInsertedId}\r审核列表：https://www.ziyuebot.cn/queue.php?id={userId}";
    }

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (args.Length < 2) return "参数数量不足。使用“/help 扔云瓶”查看命令用法。";
        string arguments = string.Join(' ', args[1..]);
        if (arguments.Contains('\u2406')) return "云瓶内容禁止包含表情！";
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（每分钟 1 条）";

        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        UpdateInvokeRecords(userId);

        return Invoke(userName, userId, arguments);
    }

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (EmotionRegex().IsMatch(args[1])) return "云瓶内容禁止包含表情！";
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（每分钟 1 条）";

        Logger.Info($"调用者：{userPing} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        UpdateInvokeRecords(userId);

        return Invoke(Message.MentionedUinAndName[userId], userId, args[1]);
    }

    public override TimeSpan GetRateLimit(Platform? platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }

    [GeneratedRegex("<:.*:\\d+>")]
    public static partial Regex EmotionRegex();
}