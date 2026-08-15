using Microsoft.Data.Sqlite;

namespace ZiYueBot.Core;

public abstract class Command
{
    public virtual Platform[] SupportedPlatform => [Platform.CBMR];

    /// <summary>
    /// 命令名。
    /// </summary>
    /// <returns />
    public abstract string Id { get; }

    /// <summary>
    /// 命令列表显示的名字。
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Discord 的机器人命令简介。
    /// </summary>
    public abstract string Summary { get; }

    /// <summary>
    /// 帮助命令内显示的信息。
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// 获取该命令的调用频率限制。
    /// </summary>
    public virtual TimeSpan GetRateLimit(Context context)
    {
        return TimeSpan.Zero;
    }

    public abstract Task Invoke(Context context, MessageChain arg);

    /// <summary>
    /// 更新数据库里的命令调用记录。这一函数只适用于仅记录调用次数的命令，复杂统计数据要单开数据库表。
    /// </summary>
    protected async Task UpdateInvokeRecords(string userid)
    {
        await using SqliteConnection connection = ZiYueBot.Instance.ConnectDatabase();
        await using SqliteCommand command = new SqliteCommand($"""
                                                               INSERT INTO invoke_records_general VALUES ({userid}, '{Id}', datetime(), datetime(), 1)
                                                               ON CONFLICT(userid, command) DO UPDATE SET last_invoke = datetime(), invoke_count = invoke_count + 1
                                                               """, connection);
        await command.ExecuteNonQueryAsync();
    }
}