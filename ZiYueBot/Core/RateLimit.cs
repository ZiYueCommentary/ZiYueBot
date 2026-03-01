namespace ZiYueBot.Core;

/// <summary>
/// 频率限制相关。
/// </summary>
public static class RateLimit
{
    private static readonly Dictionary<(string, ulong userId), DateTime> LastInvoke = [];

    /// <summary>
    /// 尝试通过频率限制检查。如果通过，该函数会自动记录最后一次调用为现在时间。
    /// </summary>
    /// <returns>是否成功通过</returns>
    public static bool TryPassRateLimit(this Command command, IContext context)
    {
        return TryPassRateLimit(command.Id, context.UserId, command.GetRateLimit(context));
    }

    /// <summary>
    /// 仅通过用户 ID 尝试通过频率限制。这一函数会绕过命令设置的频率限制。
    /// </summary>
    public static bool TryPassRateLimit(string key, ulong userId, TimeSpan rateLimit)
    {
        if (Privileged.HasPrivilege(userId, Privilege.BypassRateLimit)) return true;
        DateTime last = LastInvoke.GetValueOrDefault((key, userId), DateTime.MinValue);
        DateTime now = DateTime.UtcNow;
        if (now - last < rateLimit) return false;
        LastInvoke[(key, userId)] = now;
        return true;
    }
}