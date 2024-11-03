namespace ZiYueBot.Core;

/// <summary>
/// 频率限制相关。
/// </summary>
public static class RateLimit
{
    internal static readonly Dictionary<ICommand, Dictionary<ulong, DateTime>> LastInvoke = [];

    /// <summary>
    /// 尝试通过频率限制检查。如果通过，该函数会自动记录最后一次调用为现在时间。
    /// </summary>
    /// <returns>是否成功通过</returns>
    public static bool TryPassRateLimit(ICommand command, ulong userId)
    {
        DateTime last = LastInvoke[command].GetValueOrDefault(userId, DateTime.MinValue);
        DateTime now = DateTime.UtcNow;
        if (now - last > command.GetRateLimit()) return false;
        LastInvoke[command][userId] = now;
        return true;
    }
}