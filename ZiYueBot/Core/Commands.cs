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
    /// 注册鸿蒙命令，并将其与指定的命令名绑定。该方式不会将鸿蒙命令与 ID 绑定。
    /// </summary>
    [Obsolete]
    private static void RegisterHarmonyCommand(HarmonyCommand command, params string[] names)
    {
        foreach (string name in names)
        {
            HarmonyCommands[name] = command;
        }
    }

    /// <summary>
    /// 根据鸿蒙命令的类型获取命令。该函数假设命令一定存在，否则抛出异常。
    /// </summary>
    public static T GetHarmonyCommand<T>() where T : HarmonyCommand
    {
        foreach (HarmonyCommand command in HarmonyCommands.Values)
        {
            if (command is T t)
            {
                return t;
            }
        }
        throw new KeyNotFoundException("鸿蒙命令未找到！");
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
    /// 注册一般命令，并将其与指定的命令名绑定。该方式不会将一般命令与 ID 绑定。
    /// </summary>
    [Obsolete]
    private static void RegisterGeneralCommand(GeneralCommand command, params string[] names)
    {
        foreach (string name in names)
        {
            GeneralCommands[name] = command;
        }
    }

    /// <summary>
    /// 根据一般命令类型获取命令。当未找到命令 / 所绑定的命令不支持指定平台时，返回 null。
    /// </summary>
    public static T? GetGeneralCommand<T>(Platform platform) where T : GeneralCommand
    {
        foreach (GeneralCommand command in GeneralCommands.Values)
        {
            if (command is not T t) continue;
            if (t.SupportedPlatform == Platform.Both || t.SupportedPlatform == platform)
            {
                return t;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据命令名和一般命令类型获取命令。当命令名未注册 / 或所绑定的命令不是指定类型 / 所绑定的命令不支持指定平台时，返回 null。
    /// </summary>
    public static T? GetGeneralCommand<T>(Platform platform, string name) where T : GeneralCommand
    {
        if (GeneralCommands.GetValueOrDefault(name) is not T t) return default;
        if (t.SupportedPlatform == Platform.Both || t.SupportedPlatform == platform)
        {
            return t;
        }
        return null;
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
