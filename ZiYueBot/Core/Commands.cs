using MySql.Data.MySqlClient;
using ZiYueBot.General;
using ZiYueBot.Harmony;

namespace ZiYueBot.Core;

/// <summary>
/// 命令管理相关。
/// </summary>
public static class Commands
{
    public static readonly Dictionary<string, Command> RegisteredCommands = [];

    public static void RegisterCommand(Command command)
    {
        RegisteredCommands[command.Id] = command;
    }

    public static Command? GetCommand(Platform platform, string name)
    {
        if (!RegisteredCommands.ContainsKey(name)) return null;
        return RegisteredCommands[name].SupportedPlatform.Contains(platform) ? RegisteredCommands[name] : null;
    }

    /// <summary>
    /// 检查用户是否被禁止调用特定命令，或被禁止调用子悦机器。如果被禁止，则让机器发送封禁消息。
    /// </summary>
    /// <returns>是否在黑名单内</returns>
    public static async Task<bool> CheckBlacklist(IContext context, string command)
    {
        await using MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase();
        await using MySqlCommand sqlCommand = new MySqlCommand(
            "SELECT * FROM blacklists WHERE userid = @userid AND command = @command",
            connection);

        sqlCommand.Parameters.AddWithValue("@userid", context.UserId);
        sqlCommand.Parameters.AddWithValue("@command", "all");
        await using (MySqlDataReader reader = sqlCommand.ExecuteReader())
        {
            if (reader.Read())
            {
                await context.SendMessage($"""
                                           您已被禁止使用子悦机器！
                                           时间：{reader.GetDateTime("time"):yyyy年MM月dd日 HH:mm:ss}
                                           原因：{reader.GetString("reason")}
                                           用户协议：https://docs.ziyuebot.cn/tos.html
                                           """);
                return true;
            }
        }

        sqlCommand.Parameters.Clear();
        sqlCommand.Parameters.AddWithValue("@userid", context.UserId);
        sqlCommand.Parameters.AddWithValue("@command", command);
        await using (MySqlDataReader reader = sqlCommand.ExecuteReader())
        {
            if (!reader.Read()) return false;
            await context.SendMessage($"""
                                       您已被禁止使用该命令！
                                       时间：{reader.GetDateTime("time"):yyyy年MM月dd日 HH:mm:ss}
                                       原因：{reader.GetString("reason")}
                                       用户协议：https://docs.ziyuebot.cn/tos.html
                                       """);
            return true;
        }
    }

    /// <summary>
    /// 注册命令。
    /// </summary>
    public static void Initialize()
    {
        // 鸿蒙命令
        RegisterCommand(new Jrrp());
        RegisterCommand(new Hitokoto());
        RegisterCommand(new Ask());
        RegisterCommand(new About());
        RegisterCommand(new BaLogo());
        RegisterCommand(new Quotations());
        RegisterCommand(new Xibao());
        RegisterCommand(new Beibao());
        RegisterCommand(new StartRevolver());
        RegisterCommand(new Shooting());
        RegisterCommand(new Rotating());
        RegisterCommand(new RestartRevolver());
        // 一般命令
        RegisterCommand(new Help());
        RegisterCommand(new PicFace());
        RegisterCommand(new ThrowDriftbottle());
        RegisterCommand(new PickDriftbottle());
        RegisterCommand(new RemoveDriftbottle());
        RegisterCommand(new ListDriftbottle());
        RegisterCommand(new AddStargazer());
        RegisterCommand(new ThrowStraitbottle());
        RegisterCommand(new PickStraitbottle());
        RegisterCommand(new ListStraitbottle());
        RegisterCommand(new Win());
        RegisterCommand(new Chat());
        RegisterCommand(new Draw());
        RegisterCommand(new Stat());
    }
}