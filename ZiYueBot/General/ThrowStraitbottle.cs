using System.Text.RegularExpressions;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Harmony;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class ThrowStraitbottle : IGeneralCommand
{
    public static readonly ILog Logger = LogManager.GetLogger("扔海峡云瓶");
    
    public string GetCommandId()
    {
        return "扔海峡云瓶";
    }

    public string GetCommandName()
    {
        return "扔海峡云瓶";
    }

    public string GetCommandDescription()
    {
        return """
               /扔海峡云瓶 [content]
               扔一个海峡云瓶。由 QQ 扔出的瓶子只能被 Discord 捞起，反之亦然。所有瓶子只能被捞起一次。
               “content”是瓶子的内容，要求不包含表情。
               频率限制：每次调用间隔 1 分钟。
               在线文档：https://docs.ziyuebot.cn/throw-straitbottle.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "扔一个海峡云瓶";
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (args.Length < 2) return "参数数量不足。使用“/help 扔海峡云瓶”查看命令用法。";
        if (args[1].Contains('\u2406')) return "云瓶内容禁止包含表情！";
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        
        using MySqlCommand command = new MySqlCommand("INSERT INTO straitbottles(userid, username, created, content, fromDiscord) VALUE (@userid, @username, now(), @content, false)", ZiYueBot.Instance.Database);
        command.Parameters.AddWithValue("@userid", userId);
        command.Parameters.AddWithValue("@username", userName);
        command.Parameters.AddWithValue("@content", ThrowDriftbottle.FriendlyMessage(args[1]));
        command.ExecuteNonQuery();
        return "你的海峡云瓶扔出去了！";

    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (args.Length < 2) return "参数数量不足。使用“/help 扔海峡云瓶”查看命令用法。";
        if (Regex.IsMatch(args[1], "<:.*:\\d+>")) return "云瓶内容禁止包含表情！";
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        Logger.Info($"调用者：{userPing} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        
        using MySqlCommand command = new MySqlCommand("INSERT INTO straitbottles(userid, username, created, content, fromDiscord) VALUE (@userid, @username, now(), @content, true)", ZiYueBot.Instance.Database);
        command.Parameters.AddWithValue("@userid", userId);
        command.Parameters.AddWithValue("@username", Message.MentionedUinAndName[userId]);
        command.Parameters.AddWithValue("@content", ThrowDriftbottle.FriendlyMessage(args[1]));
        command.ExecuteNonQuery();
        return "你的海峡云瓶扔出去了！";
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }
}