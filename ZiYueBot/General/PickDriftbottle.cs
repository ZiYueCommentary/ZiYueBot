using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class PickDriftbottle : IGeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("捞云瓶");

    public string GetCommandId()
    {
        return "捞云瓶";
    }

    public string GetCommandName()
    {
        return "捞云瓶";
    }

    public string GetCommandDescription()
    {
        return """
               /捞云瓶 [id]
               捞一个漂流云瓶。“id”是可选参数，为瓶子的数字编号。
               频率限制：QQ 群聊每次调用间隔 1 分钟，私聊不限；Discord 不限。
               在线文档：https://docs.ziyuebot.cn/pick-driftbottle.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "捞一个漂流云瓶";
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

    private string Invoke(int id)
    {
        using MySqlCommand command = new MySqlCommand(
            id == int.MinValue
                ? "SELECT * FROM driftbottles WHERE pickable = true ORDER BY RAND() LIMIT 1"
                : $"SELECT * FROM driftbottles WHERE pickable = true AND id = {id}",
            ZiYueBot.Instance.Database);
        using MySqlDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return "找不到瓶子！";

        string result = $"""
                         你捞到了 {reader.GetInt32("id")} 号瓶子！
                         来自：{reader.GetString("username")}
                         日期：{reader.GetDateTime("created"):yyyy年MM月dd日}

                         {reader.GetString("content")}
                         """;

        using MySqlCommand addViews =
            new MySqlCommand($"UPDATE driftbottles SET views = views + 1 WHERE id = {reader.GetInt32("id")}",
                ZiYueBot.Instance.Database);
        reader.Close();
        addViews.ExecuteNonQuery();

        return result;
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        int id = int.MinValue;
        if (args.Length > 1)
        {
            try
            {
                id = int.Parse(args[1]);
            }
            catch (FormatException)
            {
                return "请输入数字编号！";
            }
            catch (OverflowException)
            {
                return "编号过大！";
            }
        }

        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        return Invoke(id);
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        int id = int.MinValue;
        try
        {
            id = int.Parse(args[0]);
        }
        catch (FormatException)
        {
            return "请输入数字编号！";
        }
        catch (OverflowException)
        {
            return "编号过大！";
        }

        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（每分钟 0 条）";
        Logger.Info($"调用者：{userPing} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        return Invoke(id);
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        if (platform == Platform.Discord || eventType == EventType.DirectMessage) return TimeSpan.Zero;
        return TimeSpan.FromMinutes(1);
    }
}