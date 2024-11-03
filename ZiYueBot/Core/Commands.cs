using ZiYueBot.General;
using ZiYueBot.Harmony;

namespace ZiYueBot.Core;

public static class Commands
{
    public static readonly Dictionary<string, IHarmonyCommand> HarmonyCommands = [];
    public static readonly Dictionary<string, IGeneralCommand> GeneralCommands = [];

    public static void RegisterHarmonyCommand(IHarmonyCommand command)
    {
        HarmonyCommands[command.GetCommandID()] = command;
        RateLimit.LastInvoke.TryAdd(command, []);
    }

    public static void RegisterHarmonyCommand(IHarmonyCommand command, params string[] names)
    {
        foreach (string name in names)
        {
            HarmonyCommands[name] = command;
        }
        RateLimit.LastInvoke.TryAdd(command, []);
    }

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

    public static T? GetHarmonyCommand<T>(string name) where T : IHarmonyCommand
    {
        if (HarmonyCommands.GetValueOrDefault(name) is T t)
        {
            return t;
        }
        return default;
    }

    public static void RegisterGeneralCommand(IGeneralCommand command)
    {
        GeneralCommands[command.GetCommandID()] = command;
        RateLimit.LastInvoke.TryAdd(command, []);
    }

    public static void RegisterGeneralCommand(IGeneralCommand command, params string[] names)
    {
        foreach (string name in names)
        {
            GeneralCommands[name] = command;
        }
        RateLimit.LastInvoke.TryAdd(command, []);
    }

    public static T? GetGeneralCommand<T>(Platform platform) where T : IGeneralCommand
    {
        foreach (IGeneralCommand command in GeneralCommands.Values)
        {
            if (command is T t)
            {
                if (t.GetSupportedPlatform() == Platform.Both || t.GetSupportedPlatform() == platform)
                {
                    return t;
                }
            }
        }
        return default;
    }

    public static T? GetGeneralCommand<T>(Platform platform, string name) where T : IGeneralCommand
    {
        if (GeneralCommands.GetValueOrDefault(name) is T t)
        {
            if (t.GetSupportedPlatform() == Platform.Both || t.GetSupportedPlatform() == platform)
            {
                return t;
            }
        }
        return default;
    }

    public static void Initialize()
    {
        RegisterHarmonyCommand(new Jrrp());
        RegisterHarmonyCommand(new Hitokoto());
        RegisterHarmonyCommand(new Ask());

        RegisterGeneralCommand(new Help());
        RegisterGeneralCommand(new PicFace());
    }
}
