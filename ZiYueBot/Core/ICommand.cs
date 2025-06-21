using ZiYueBot.General;

namespace ZiYueBot.Core;

public interface ICommand
{
    /// <summary>
    /// 命令名。
    /// </summary>
    /// <returns />
    string GetCommandId();

    /// <summary>
    /// 命令列表显示的名字。
    /// </summary>
    string GetCommandName();

    /// <summary>
    /// 帮助命令内显示的信息。
    /// </summary>
    string GetCommandDescription();

    /// <summary>
    /// Discord 的机器人命令描述。
    /// </summary>
    string GetCommandShortDescription();

    /// <summary>
    /// 获取该命令的调用频率限制，用户无关型。
    /// </summary>
    TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.Zero;
    }

    /// <summary>
    /// 获取该命令的调用频率限制。
    /// </summary>
    TimeSpan GetRateLimit(Platform platform, EventType eventType, ulong userId)
    {
        return GetRateLimit(platform, eventType);
    }
}