namespace ZiYueBot.Core;

public class RateLimit
{
    internal static readonly Dictionary<ICommand, Dictionary<ulong, long>> LastInvoke = [];

    public static bool IsLimited(ICommand command, ulong userId)
    {
        long last = LastInvoke[command].GetValueOrDefault(userId, 0);
        long now = DateTimeOffset.Now.ToUnixTimeSeconds();
        if (now - last >= command.GetRateLimit())
        {
            LastInvoke[command][userId] = now;
            return false;
        }
        return true;
    }
}
