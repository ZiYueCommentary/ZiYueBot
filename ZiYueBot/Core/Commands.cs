using MySql.Data.MySqlClient;
using ZiYueBot.General;
using ZiYueBot.Harmony;

namespace ZiYueBot.Core;

/// <summary>
/// 命令管理相关。
/// </summary>
public static class Commands
{
    public static readonly Dictionary<string, HarmonyCommand> HarmonyCommands = [];
    public static readonly Dictionary<string, GeneralCommand> GeneralCommands = [];

    /// <summary>
    /// 注册鸿蒙命令，并将该命令与 ID 绑定。
    /// </summary>
    private static void RegisterHarmonyCommand(HarmonyCommand command)
    {
        HarmonyCommands[command.Id] = command;
    }

    /// <summary>
    /// 根据命令名和鸿蒙命令类型获取命令。当命令名未注册，或所绑定的命令不是指定类型时，返回 null。
    /// </summary>
    public static T? GetHarmonyCommand<T>(string name) where T : HarmonyCommand
    {
        if (HarmonyCommands.GetValueOrDefault(name) is T t)
        {
            return t;
        }

        return null;
    }

    /// <summary>
    /// 注册一般命令，并将该命令与 ID 绑定。
    /// </summary>
    private static void RegisterGeneralCommand(GeneralCommand command)
    {
        GeneralCommands[command.Id] = command;
    }

    /// <summary>
    /// 根据命令名和一般命令类型获取命令。当命令名未注册 / 或所绑定的命令不是指定类型 / 所绑定的命令不支持指定平台时，返回 null。
    /// </summary>
    public static T? GetGeneralCommand<T>(Platform platform, string name) where T : GeneralCommand
    {
        if (GeneralCommands.GetValueOrDefault(name) is not T t) return null;
        return t.SupportedPlatform.Contains(platform) ? t : null;
    }

    /// <summary>
    /// 检查用户是否被禁止调用特定命令，或被禁止调用子悦机器。
    /// </summary>
    /// <param name="message">当返回值为真时，返回的禁止调用的原因</param>
    /// <returns>本次调用是否允许</returns>
    public static bool CheckBlacklist(ulong userId, string command, out string message)
    {
        using (MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase())
        {
            using MySqlCommand sqlCommand = new MySqlCommand(
                $"SELECT * FROM blacklists WHERE userid = {userId} AND command = 'all'",
                connection);
            using MySqlDataReader reader = sqlCommand.ExecuteReader();
            if (reader.Read())
            {
                message = $"""
                           您已被禁止使用子悦机器！
                           时间：{reader.GetDateTime("time"):yyyy年MM月dd日 HH:mm:ss}
                           原因：{reader.GetString("reason")}
                           用户协议：https://docs.ziyuebot.cn/tos.html
                           """;
                return true;
            }
        }

        using (MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase())
        {
            using MySqlCommand sqlCommand = new MySqlCommand(
                "SELECT * FROM blacklists WHERE userid = @userid AND command = @command",
                connection);
            sqlCommand.Parameters.AddWithValue("@userid", userId);
            sqlCommand.Parameters.AddWithValue("@command", command);
            using MySqlDataReader reader = sqlCommand.ExecuteReader();
            if (reader.Read())
            {
                message = $"""
                           您已被禁止使用该命令！
                           时间：{reader.GetDateTime("time"):yyyy年MM月dd日 HH:mm:ss}
                           原因：{reader.GetString("reason")}
                           用户协议：https://docs.ziyuebot.cn/tos.html
                           """;
                return true;
            }
        }

        message = "";
        return false;
    }

    /// <summary>
    /// 注册命令。
    /// </summary>
    public static void Initialize()
    {
        RegisterHarmonyCommand(new Jrrp());
        RegisterHarmonyCommand(new Hitokoto());
        RegisterHarmonyCommand(new Ask());
        RegisterHarmonyCommand(new About());
        RegisterHarmonyCommand(new BALogo());
        RegisterHarmonyCommand(new Quotations());
        RegisterHarmonyCommand(new Xibao());
        RegisterHarmonyCommand(new Beibao());
        RegisterHarmonyCommand(new StartRevolver());
        RegisterHarmonyCommand(new Shooting());
        RegisterHarmonyCommand(new Rotating());
        RegisterHarmonyCommand(new RestartRevolver());

        RegisterGeneralCommand(new Help());
        RegisterGeneralCommand(new PicFace());
        RegisterGeneralCommand(new ThrowDriftbottle());
        RegisterGeneralCommand(new PickDriftbottle());
        RegisterGeneralCommand(new RemoveDriftbottle());
        RegisterGeneralCommand(new ListDriftbottle());
        RegisterGeneralCommand(new ThrowStraitbottle());
        RegisterGeneralCommand(new PickStraitbottle());
        RegisterGeneralCommand(new ListStraitbottle());
        RegisterGeneralCommand(new Win());
        RegisterGeneralCommand(new Chat());
        RegisterGeneralCommand(new Draw());
        RegisterGeneralCommand(new Stat());
    }
}