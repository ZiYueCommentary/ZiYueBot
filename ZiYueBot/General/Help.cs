﻿using log4net;
using ZiYueBot.Core;
using ZiYueBot.Harmony;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class Help : IGeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("帮助");

    public string GetCommandDescription()
    {
        return """
               /help [command]
               获取命令的帮助信息。其中“command”为命令名，为空时返回可用命令列表。
               在线文档：https://docs.ziyuebot.cn/general/help
               """;
    }

    public string GetCommandId()
    {
        return "help";
    }

    public string GetCommandName()
    {
        return "帮助";
    }

    public string GetCommandShortDescription()
    {
        return "获取帮助";
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

    private string Invoke(Platform platform, string userName, ulong userId, string[] args)
    {
        Logger.Info($"平台：${platform}，调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        if (args.Length >= 2 && args[1] != "")
        {
            IHarmonyCommand? harmony = Commands.GetHarmonyCommand<IHarmonyCommand>(args[1]);
            if (harmony is not null)
            {
                return harmony.GetCommandDescription();
            }

            IGeneralCommand? general = Commands.GetGeneralCommand<IGeneralCommand>(platform, args[1]);
            return general is not null ? general.GetCommandDescription() : $"命令未找到：{args[1]}";
        }

        string help = Commands.HarmonyCommands.Values.ToHashSet().Aggregate("子悦机器可用命令：\n",
            (current, command) => current + $"\t/{command.GetCommandId()}\t{command.GetCommandName()}\n");

        help = Commands.GeneralCommands.Values.ToHashSet()
            .Where(command => command.GetSupportedPlatform() == Platform.Both ||
                              command.GetSupportedPlatform() == platform).Aggregate(help,
                (current, command) => current + $"\t/{command.GetCommandId()}\t{command.GetCommandName()}\n");

        help += "输入“/help [命令名]”可以查看命令帮助。\n详细信息请查看在线文档：https://docs.ziyuebot.cn/";
        return help;
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        return Invoke(Platform.QQ, userName, userId, args);
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        return Invoke(Platform.Discord, userPing, userId, args);
    }
}