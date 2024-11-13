using ZiYueBot.General;
using ZiYueBot.Harmony;

namespace ZiYueBot.Core;

/// <summary>
/// 命令管理相关。
/// </summary>

public static class Commands
{
    public static readonly Dictionary<string, IHarmonyCommand> HarmonyCommands = [];
    public static readonly Dictionary<string, IGeneralCommand> GeneralCommands = [];

    /// <summary>
    /// 注册鸿蒙命令，并将该命令与GetCommandID()的命令名绑定。
    /// </summary>
    private static void RegisterHarmonyCommand(IHarmonyCommand command)
    {
        HarmonyCommands[command.GetCommandId()] = command;
    }

    /// <summary>
    /// 注册鸿蒙命令，并将其与指定的命令名绑定。该方式不会将鸿蒙命令与GetCommandId()的命令名绑定。
    /// </summary>
    private static void RegisterHarmonyCommand(IHarmonyCommand command, params string[] names)
    {
        foreach (string name in names)
        {
            HarmonyCommands[name] = command;
        }
    }

    /// <summary>
    /// 根据鸿蒙命令的类型获取命令。该函数假设命令一定存在，否则抛出异常。
    /// </summary>
    public static T GetHarmonyCommand<T>() where T : IHarmonyCommand
    {
        foreach (IHarmonyCommand command in HarmonyCommands.Values)
        {
            if (command is T t)
            {
                return t;
            }
        }
        throw new KeyNotFoundException($"鸿蒙命令未找到！");
    }

    /// <summary>
    /// 根据命令名和鸿蒙命令类型获取命令。当命令名未注册，或所绑定的命令不是指定类型时，返回null。
    /// </summary>
    public static T? GetHarmonyCommand<T>(string name) where T : IHarmonyCommand
    {
        if (HarmonyCommands.GetValueOrDefault(name) is T t)
        {
            return t;
        }
        return default;
    }

    /// <summary>
    /// 注册一般命令，并将该命令与GetCommandID()的命令名绑定。
    /// </summary>
    private static void RegisterGeneralCommand(IGeneralCommand command)
    {
        GeneralCommands[command.GetCommandId()] = command;
    }

    /// <summary>
    /// 注册一般命令，并将其与指定的命令名绑定。该方式不会将一般命令与GetCommandId()的命令名绑定。
    /// </summary>
    private static void RegisterGeneralCommand(IGeneralCommand command, params string[] names)
    {
        foreach (string name in names)
        {
            GeneralCommands[name] = command;
        }
    }

    /// <summary>
    /// 根据一般命令类型获取命令。当未找到命令 / 所绑定的命令不支持指定平台时，返回null。
    /// </summary>
    public static T? GetGeneralCommand<T>(Platform platform) where T : IGeneralCommand
    {
        foreach (IGeneralCommand command in GeneralCommands.Values)
        {
            if (command is not T t) continue;
            if (t.GetSupportedPlatform() == Platform.Both || t.GetSupportedPlatform() == platform)
            {
                return t;
            }
        }
        return default;
    }

    /// <summary>
    /// 根据命令名和一般命令类型获取命令。当命令名未注册 / 或所绑定的命令不是指定类型 / 所绑定的命令不支持指定平台时，返回null。
    /// </summary>
    public static T? GetGeneralCommand<T>(Platform platform, string name) where T : IGeneralCommand
    {
        if (GeneralCommands.GetValueOrDefault(name) is not T t) return default;
        if (t.GetSupportedPlatform() == Platform.Both || t.GetSupportedPlatform() == platform)
        {
            return t;
        }
        return default;
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
        RegisterHarmonyCommand(new ThrowDriftbottle());
        RegisterHarmonyCommand(new PickDriftbottle());

        RegisterGeneralCommand(new Help());
        RegisterGeneralCommand(new PicFace());
    }
}
