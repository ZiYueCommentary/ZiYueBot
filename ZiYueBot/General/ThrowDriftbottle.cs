using System.Text.RegularExpressions;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class ThrowDriftbottle : IGeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("扔云瓶");

    public string GetCommandId()
    {
        return "扔云瓶";
    }

    public string GetCommandName()
    {
        return "扔云瓶";
    }

    public string GetCommandDescription()
    {
        return """
               /扔云瓶 [content]
               扔一个漂流云瓶。“content”是瓶子的内容，要求不包含表情。
               频率限制：每次调用间隔 1 分钟。
               在线文档：https://docs.ziyuebot.cn/general/driftbottle/throw
               """;
    }

    public string GetCommandShortDescription()
    {
        return "扔一个漂流云瓶";
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

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
                "INSERT INTO driftbottles(userid, username, created, content) VALUE (@userid, @username, now(), @content)",
                database);
        command.Parameters.AddWithValue("@userid", userId);
        command.Parameters.AddWithValue("@username", userName);
        command.Parameters.AddWithValue("@content", FriendlyMessage(content));
        command.ExecuteNonQuery();
        return $"你的 {command.LastInsertedId} 号漂流瓶扔出去了！";
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (args.Length < 2) return "参数数量不足。使用“/help 扔云瓶”查看命令用法。";
        string arguments = string.Join(' ', args[1..]);
        if (arguments.Contains('\u2406')) return "云瓶内容禁止包含表情！";
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");

        return Invoke(userName, userId, arguments);
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (Regex.IsMatch(args[0], "<:.*:\\d+>")) return "云瓶内容禁止包含表情！";
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        Logger.Info($"调用者：{userPing} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");

        return Invoke(Message.MentionedUinAndName[userId], userId, args[0]);
    }

    public static string FriendlyMessage(string arg)
    {
        string result = "";
        bool simpleMessage = true;
        int pos = 0;
        for (int i = 0; i < arg.Length; i++)
        {
            switch (arg[i])
            {
                case '\u2402': // 图片
                {
                    result += arg.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                    int end = arg.IndexOf('\u2403', i + 1);
                    string path = $"data/images/{Guid.NewGuid()}.png";
                    WebUtils.DownloadFile(arg.Substring(i + 1, end - i - 1), path).GetAwaiter().GetResult();
                    result += $"\u2408{path}\u2409";
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
                case '\u2404': // 提及
                {
                    result += arg.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                    int end = arg.IndexOf('\u2405', i + 1);
                    result += $" {Message.MentionedUinAndName[ulong.Parse(arg.Substring(i + 1, end - i - 1))]} ";
                    if (i == 0) result = result[1..];
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
                case '<': // Discord 提及
                {
                    result += arg.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                    if (arg.IndexOf('@', i + 1) != i + 1)
                    {
                        continue;
                    }

                    int end = arg.IndexOf('>', i + 1);
                    result += $" {Message.MentionedUinAndName[ulong.Parse(arg.Substring(i + 2, end - i - 2))]} ";
                    if (i == 0) result = result[1..];
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
            }
        }

        if (simpleMessage) return arg;

        if (pos < arg.Length - 1) result += arg[(pos + (arg[pos + 1] == ' ' ? 2 : 1))..];
        return result;
    }
}