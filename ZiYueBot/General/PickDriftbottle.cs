using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class PickDriftbottle : GeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("捞云瓶");

    public override string Id => "捞云瓶";

    public override string Name => "捞云瓶";

    public override string Summary => "捞一个漂流云瓶";

    public override string Description => """
                                          /捞云瓶 [id]
                                          捞一个漂流云瓶。“id”是可选参数，为瓶子的数字编号。
                                          频率限制：QQ 群聊每次调用间隔 1 分钟，私聊不限；Discord 不限。
                                          云瓶生态建设条例：https://docs.ziyuebot.cn/tos-driftbottle
                                          在线文档：https://docs.ziyuebot.cn/general/driftbottle/pick
                                          """;

    private string Invoke(int id)
    {
        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1) // 愚人节！
        {
            if (Random.Shared.Next(2) == 1) // 50% 概率捞到愚人云瓶
            {
                using MySqlCommand aprilCommand = new MySqlCommand(
                    "SELECT * FROM aprilbottles ORDER BY RAND() LIMIT 1",
                    database);
                using MySqlDataReader aprilReader = aprilCommand.ExecuteReader();
                if (!aprilReader.Read()) return "找不到愚人云瓶！";

                return $"""
                        你捞到了 -{aprilReader.GetInt32("id")} 号瓶子！
                        来自：{aprilReader.GetString("username")}
                        日期：{aprilReader.GetDateTime("created"):yyyy年MM月dd日}

                        {aprilReader.GetString("content")}
                        """;
            }
        }

        if (id == 0) id = 625; // 说的道理~

        using MySqlCommand command = new MySqlCommand(
            id == int.MinValue
                ? "SELECT * FROM driftbottles WHERE pickable = true ORDER BY RAND() LIMIT 1"
                : $"SELECT * FROM driftbottles WHERE pickable = true AND id = {id}",
            database);
        using MySqlDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return "找不到瓶子！";

        int stargazers = Stargazers.GetStargazerCount(id);
        string result = $"""
                         你捞到了 {reader.GetInt32("id")} 号瓶子！
                         来自：{reader.GetString("username")}
                         日期：{reader.GetDateTime("created"):yyyy年MM月dd日}
                         """ +
                        (stargazers > 0 ? $"\n星标数：{stargazers}" : "") +
                        $"""

                         {reader.GetString("content")}
                         """;

        using MySqlCommand addViews = new MySqlCommand(
            $"UPDATE driftbottles SET views = views + 1 WHERE id = {reader.GetInt32("id")}",
            database);
        reader.Close();
        addViews.ExecuteNonQuery();

        return result;
    }

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
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
        UpdateInvokeRecords(userId);

        return Invoke(id);
    }

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
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
        UpdateInvokeRecords(userId);

        return Invoke(id);
    }

    public override TimeSpan GetRateLimit(Platform? platform, EventType eventType)
    {
        if (platform == Platform.Discord || eventType == EventType.DirectMessage) return TimeSpan.Zero;
        return TimeSpan.FromMinutes(1);
    }
}