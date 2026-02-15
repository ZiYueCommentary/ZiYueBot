namespace ZiYueBot.Core;

/// <summary>
/// 频率限制相关。
/// </summary>
public static class RateLimit
{
    private static readonly Dictionary<(Command, ulong userId), DateTime> LastInvoke = [];

    /// <summary>
    /// 尝试通过频率限制检查。如果通过，该函数会自动记录最后一次调用为现在时间。
    /// </summary>
    /// <returns>是否成功通过</returns>
    public static bool TryPassRateLimit(this Command command, IContext context)
    {
        DateTime last = LastInvoke.GetValueOrDefault((command, context.UserId), DateTime.MinValue);
        DateTime now = DateTime.UtcNow;
        if (now - last < command.GetRateLimit(context)) return false;
        LastInvoke[(command, context.UserId)] = now;
        return true;
    }

    /// <summary>
    /// 仅通过用户 ID 尝试通过频率限制。这一函数会绕过命令设置的频率限制。
    /// </summary>
    public static bool TryPassRateLimit(Command command, ulong userId, TimeSpan rateLimit)
    {
        DateTime last = LastInvoke.GetValueOrDefault((command, userId), DateTime.MinValue);
        DateTime now = DateTime.UtcNow;
        if (now - last < rateLimit) return false;
        LastInvoke[(command, userId)] = now;
        return true;
    }
}