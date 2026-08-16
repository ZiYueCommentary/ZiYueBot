using Microsoft.Data.Sqlite;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class PickDriftbottle : Command
{
    // private static readonly ILog Logger = LogManager.GetLogger("捞云瓶");

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

    public override async Task Invoke(Context context, MessageChain arg)
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

         // Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
         _ = UpdateInvokeRecords(context.UserId);

         await using SqliteConnection database = ZiYueBot.Instance.ConnectDatabase();

         string query;
         if (id == int.MinValue)
         {
             await using SqliteCommand queryCount = new SqliteCommand("SELECT COUNT(*) FROM driftbottles", database);
             await using SqliteDataReader readerCount = await queryCount.ExecuteReaderAsync();
             readerCount.Read();
             int counts = readerCount.GetInt32(0);
             int begin = Random.Shared.Next(1, Math.Max(1, counts - 10));
             await readerCount.CloseAsync();
             query =
                 $"SELECT (id, username, created, content) FROM driftbottles WHERE id >= {begin} AND id < {begin + 10} AND pickable = true ORDER BY random() LIMIT 1";
         }
         else
         {
             query = $"SELECT (id, username, created, content) FROM driftbottles WHERE pickable = true AND id = {id}";
         }

         await using SqliteCommand command = new SqliteCommand(query, database);
         await using SqliteDataReader reader = await command.ExecuteReaderAsync();
         if (!reader.Read())
         {
             await context.SendMessage("找不到瓶子！");
             return;
         }

         string result = $"""
                          你捞到了 {reader.GetInt32(0)} 号瓶子！
                          来自：{reader.GetString(1)}
                          日期：{reader.GetDateTime(2):yyyy年MM月dd日}
                          
                          {reader.GetString(3)}
                          """;

         await using SqliteCommand addViews = new SqliteCommand(
             $"UPDATE driftbottles SET views = views + 1 WHERE id = {reader.GetInt32(0)}",
             database);
         await reader.CloseAsync();
         addViews.ExecuteNonQuery();

         await context.SendMessage(result);
    }

    public override TimeSpan GetRateLimit(Context context)
    {
        return TimeSpan.FromMinutes(1);
    }
}