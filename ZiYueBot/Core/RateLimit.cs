using ZiYueBot.General;

namespace ZiYueBot.Core;

/// <summary>
/// 频率限制相关。
/// </summary>
public static class RateLimit
{
    private static readonly Dictionary<(ICommand, Platform, EventType, ulong), DateTime> LastInvoke = [];

    /// <summary>
    /// 尝试通过频率限制检查。如果通过，该函数会自动记录最后一次调用为现在时间。
    /// </summary>
    /// <returns>是否成功通过</returns>
    public static bool TryPassRateLimit(ICommand command, EventType eventType, ulong userId)
    {
        return TryPassRateLimit(command, Platform.Both, eventType, userId);
    }
    
    public static bool TryPassRateLimit(ICommand command, Platform platform, EventType eventType, ulong userId)
    {
        DateTime last = LastInvoke.GetValueOrDefault((command, platform, eventType, userId), DateTime.MinValue);
        DateTime now = DateTime.UtcNow;
        if (now - last < command.GetRateLimit(platform, eventType, userId)) return false;
        LastInvoke[(command, platform, eventType, userId)] = now;
        return true;
    }
}