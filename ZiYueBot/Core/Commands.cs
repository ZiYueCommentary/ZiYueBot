using ZiYueBot.General;
using ZiYueBot.Harmony;

namespace ZiYueBot.Core;

/// <summary>
/// 命令管理相关。
/// </summary>
public static class Commands
{
    public static readonly Dictionary<string, GeneralCommand> RegisteredCommands = [];

    /// <summary>
    /// 注册命令，并将该命令与 ID 绑定。
    /// </summary>
    private static void RegisterCommand(GeneralCommand command)
    {
        RegisteredCommands[command.Id] = command;
    }

    /// <summary>
    /// 根据命令名和命令获取命令。当命令名未注册 / 所绑定的命令不支持指定平台时，返回 null。
    /// </summary>
    public static GeneralCommand? GetCommand(Platform platform, string name)
    {
        GeneralCommand? command = RegisteredCommands.GetValueOrDefault(name);
        if (command != null && (command.SupportedPlatform == Platform.Both || command.SupportedPlatform == platform))
        {
            return command;
        }

        return null;
    }

    /// <summary>
    /// 注册命令。
    /// </summary>
    public static void Initialize()
    {
        RegisterCommand(new Jrrp());
        RegisterCommand(new Hitokoto());
        RegisterCommand(new Ask());
        RegisterCommand(new About());
        RegisterCommand(new BALogo());
        RegisterCommand(new Quotations());
        RegisterCommand(new Xibao());
        RegisterCommand(new Beibao());
        RegisterCommand(new StartRevolver());
        RegisterCommand(new Shooting());
        RegisterCommand(new Rotating());
        RegisterCommand(new RestartRevolver());

        RegisterCommand(new Help());
        RegisterCommand(new PicFace());
        RegisterCommand(new ThrowDriftbottle());
        RegisterCommand(new PickDriftbottle());
        RegisterCommand(new RemoveDriftbottle());
        RegisterCommand(new ListDriftbottle());
        RegisterCommand(new ThrowStraitbottle());
        RegisterCommand(new PickStraitbottle());
        RegisterCommand(new ListStraitbottle());
        RegisterCommand(new Win());
        RegisterCommand(new Chat());
        RegisterCommand(new Draw());
        RegisterCommand(new Stat());
    }
}