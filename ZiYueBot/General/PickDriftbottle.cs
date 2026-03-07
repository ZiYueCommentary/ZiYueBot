using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class PickDriftbottle : Command
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

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        int id = int.MinValue;
        if (!arg.IsEmpty())
        {
            try
            {
                id = int.Parse(arg.ToString());
            }
            catch (FormatException)
            {
                await context.SendMessage("请输入数字编号！");
                return;
            }
            catch (OverflowException)
            {
                await context.SendMessage("编号过大！");
                return;
            }
        }

        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage("频率已达限制（每分钟 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);

        await using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1) // 愚人节！
        {
            if (Random.Shared.Next(2) == 1) // 50% 概率捞到愚人云瓶
            {
                await using MySqlCommand aprilCommand = new MySqlCommand(
                    "SELECT * FROM aprilbottles ORDER BY RAND() LIMIT 1", database);
                await using MySqlDataReader aprilReader = aprilCommand.ExecuteReader();
                if (aprilReader.Read())
                {
                    await context.SendMessage($"""
                                               你捞到了 -{aprilReader.GetInt32("id")} 号瓶子！
                                               来自：{aprilReader.GetString("username")}
                                               日期：{aprilReader.GetDateTime("created"):yyyy年MM月dd日}

                                               {aprilReader.GetString("content")}
                                               """);
                    return;
                }
            }
        }

        if (id == 0) id = 625; // 说的道理~

        string query;
        if (id == int.MinValue)
        {
            await using MySqlCommand queryCount = new MySqlCommand("SELECT COUNT(*) FROM driftbottles", database);
            await using MySqlDataReader readerCount = queryCount.ExecuteReader();
            readerCount.Read();
            int counts = readerCount.GetInt32(0);
            int begin = Random.Shared.Next(1, Math.Max(1, counts - 10));
            await readerCount.CloseAsync();
            query =
                $"SELECT * FROM driftbottles WHERE id >= {begin} AND id < {begin + 10} AND pickable = true ORDER BY RAND() LIMIT 1";
        }
        else
        {
            query = $"SELECT * FROM driftbottles WHERE pickable = true AND id = {id}";
        }

        await using MySqlCommand command = new MySqlCommand(query, database);
        await using MySqlDataReader reader = command.ExecuteReader();
        if (!reader.Read())
        {
            await context.SendMessage("找不到瓶子！");
            return;
        }

        int stargazers = Stargazers.GetStargazerCount(id);
        string result = $"""
                         你捞到了 {reader.GetInt32("id")} 号瓶子！
                         来自：{reader.GetString("username")}
                         日期：{reader.GetDateTime("created"):yyyy年MM月dd日}
                         """ +
                        (stargazers > 0 ? $"\n星标数：{stargazers}" : "") +
                        $"\n\n{reader.GetString("content")}";

        await using MySqlCommand addViews = new MySqlCommand(
            $"UPDATE driftbottles SET views = views + 1 WHERE id = {reader.GetInt32("id")}",
            database);
        await reader.CloseAsync();
        addViews.ExecuteNonQuery();

        await context.SendMessage(MessageChain.FromDatabase(result));
    }

    public override TimeSpan GetRateLimit(IContext context)
    {
        if (context.Platform == Platform.Discord || context.EventType == EventType.DirectMessage) return TimeSpan.Zero;
        return TimeSpan.FromMinutes(1);
    }
}