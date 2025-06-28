using MySql.Data.MySqlClient;
using ZiYueBot.General;

namespace ZiYueBot.Core;

public abstract class Command
{
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
    /// 获取该命令的调用频率限制，用户无关型。
    /// </summary>
    public virtual TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.Zero;
    }

    /// <summary>
    /// 获取该命令的调用频率限制。
    /// </summary>
    public virtual TimeSpan GetRateLimit(Platform platform, EventType eventType, ulong userId)
    {
        return GetRateLimit(platform, eventType);
    }
}