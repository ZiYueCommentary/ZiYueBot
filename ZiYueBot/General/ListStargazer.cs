using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class ListStargazer : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("查看星标云瓶");

    public override string Id => "查看星标云瓶";
    public override string Name => "查看星标云瓶";
    public override string Summary => "查看星标云瓶";
    public override string Description => "";

    public override async Task Invoke(Context context, MessageChain arg)
    {
        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage(
                context.EventType == EventType.DirectMessage || context.Platform == Platform.Discord
                    ? "频率已达限制（10 分钟 1 条）"
                    : "频率已达限制（30 分钟 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})");
        _ = UpdateInvokeRecords(context.UserId);

        await using MySqlCommand query = new MySqlCommand(
            $"SELECT * FROM stargazers WHERE userid = {context.UserId} AND removed = false",
            ZiYueBot.Instance.ConnectDatabase());
        await using MySqlDataReader reader = query.ExecuteReader();
        string result = $"{context.UserName} 的星标云瓶列表：\n";
        int count = 0;
        while (reader.Read())
        {
            if (count <= 50)
                result += $"- 云瓶：{reader.GetInt32("bottle_id")}，时间：{reader.GetDateTime("star_at"):yyyy-MM-dd}\n";
            count++;
        }

        result += count > 50 ? $"共 {count} 条，仅显示最早 50 条。" : $"共 {count} 条";
        await context.SendMessage(result);
    }

    public override TimeSpan GetRateLimit(Context context)
    {
        return context.EventType == EventType.DirectMessage || context.Platform == Platform.Discord
            ? TimeSpan.FromMinutes(10)
            : TimeSpan.FromMinutes(30);
    }
}