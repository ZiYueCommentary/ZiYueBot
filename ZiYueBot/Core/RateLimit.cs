using ZiYueBot.General;

namespace ZiYueBot.Core;

/// <summary>
/// 频率限制相关。
/// </summary>
public static class RateLimit
{
    private static readonly Dictionary<(Command, Platform?, EventType, ulong), DateTime> LastInvoke = [];

    /// <summary>
    /// 尝试通过频率限制检查。如果通过，该函数会自动记录最后一次调用为现在时间。
    /// </summary>
    /// <returns>是否成功通过</returns>
    public static bool TryPassRateLimit(Command command, EventType eventType, ulong userId)
    {
        return TryPassRateLimit(command, null, eventType, userId);
    }

    public static bool TryPassRateLimit(Command command, Platform? platform, EventType eventType, ulong userId)
    {
        DateTime last = LastInvoke.GetValueOrDefault((command, platform, eventType, userId), DateTime.MinValue);
        DateTime now = DateTime.UtcNow;
        if (now - last < command.GetRateLimit(platform, eventType, userId)) return false;
        LastInvoke[(command, platform, eventType, userId)] = now;
        return true;
    }
}