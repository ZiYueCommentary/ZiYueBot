using MySql.Data.MySqlClient;

namespace ZiYueBot.Core;

/// <summary>
/// 特权，管理系统的一部分。
/// </summary>
public enum Privilege : long
{
    RemoveDriftbottle = 0b1,
    BypassRateLimit = 0b10,
    BypassDrawLimitation = 0b100,
    BypassDriftbottleQueue = 0b1000
}

public static class Privileged
{
    public static long GetPrivilege(ulong userId)
    {
        using MySqlCommand command = new MySqlCommand($"SELECT * FROM privileges WHERE userid = {userId}",
            ZiYueBot.Instance.ConnectDatabase());
        using MySqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? reader.GetInt64("flags") : 0;
    }

    public static bool HasPrivilege(ulong userId, params Privilege[] privileges)
    {
        long privileged = GetPrivilege(userId);
        return privileges.All(privilege => (privileged & (long)privilege) != 0);
    }
}