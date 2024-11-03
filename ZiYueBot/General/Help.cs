using log4net;
using ZiYueBot.Core;
using ZiYueBot.Harmony;

namespace ZiYueBot.General;

public class Help : IGeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("帮助");

    public string GetCommandDescription()
    {
        return """
/help [command]
获取命令的帮助信息。其中“command”为命令名，为空时返回可用命令列表。
在线文档：https://docs.ziyuebot.cn/help.html
""";
    }

    public string GetCommandID()
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

    public string Invoke(Platform platform, string userName, ulong userId, string[] args)
    {
        Logger.Info($"平台：${platform}，调用者：{userName}（{userId}），参数：{string.Join(',', args)}");
        if (args.Length >= 2 && args[1] != "")
        {
            IHarmonyCommand? harmony = Commands.GetHarmonyCommand<IHarmonyCommand>(args[1]);
            if (harmony is not null)
            {
                return harmony.GetCommandDescription();
            }
            IGeneralCommand? general = Commands.GetGeneralCommand<IGeneralCommand>(platform, args[1]);
            if (general is not null)
            {
                return general.GetCommandDescription();
            }
            return $"命令未找到：{args[1]}";
        }
        else
        {
            string help = "子悦机器可用命令：\n";
            foreach (IHarmonyCommand command in Commands.HarmonyCommands.Values.ToHashSet())
            {
                help += $"\t/{command.GetCommandID()}\t{command.GetCommandName()}\n";
            }
            foreach (IGeneralCommand command in Commands.GeneralCommands.Values.ToHashSet())
            {
                if (command.GetSupportedPlatform() == Platform.Both || command.GetSupportedPlatform() == platform)
                {
                    help += $"\t/{command.GetCommandID()}\t{command.GetCommandName()}\n";
                }
            }
            help += "输入“/help [命令名]”可以查看命令帮助。\n详细信息请查看在线文档：https://docs.ziyuebot.cn/";
            return help;
        }
    }

    public string QQInvoke(string userName, uint userId, string[] args)
    {
        return Invoke(Platform.QQ, userName, userId, args);
    }

    public string DiscordInvoke(string userPing, ulong userId, string[] args)
    {
        return Invoke(Platform.Discord, userPing, userId, args);
    }
}
