namespace ZiYueBot.Core;

/// <summary>
/// 频率限制相关。
/// </summary>

public static class RateLimit
{
    internal static readonly Dictionary<ICommand, Dictionary<ulong, DateTime>> LastInvoke = [];

    /// <summary>
    /// 返回该用户是否处于指定命令的频率限制中。如果不处于限制中，该函数会自动记录最后一次调用为现在时间。
    /// </summary>
    public static bool IsLimited(ICommand command, ulong userId)
    {
        DateTime last = LastInvoke[command].GetValueOrDefault(userId, DateTime.MinValue);
        DateTime now = DateTime.UtcNow;
        if (now - last > command.GetRateLimit()) return true;
        LastInvoke[command][userId] = now;
        return false;
    }
}
