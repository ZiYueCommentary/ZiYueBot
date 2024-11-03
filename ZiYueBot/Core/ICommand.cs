namespace ZiYueBot.Core;

public interface ICommand
{
    /// <summary>
    /// 命令名。
    /// </summary>
    /// <returns />
    string GetCommandID();

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
    /// 获取该命令的调用频率限制。以秒为单位。
    /// </summary>
    long GetRateLimit()
    {
        return 0;
    }
}