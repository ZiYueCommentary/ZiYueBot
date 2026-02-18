using log4net;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class Help : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("帮助");

    public override string Id => "help";

    public override string Name => "帮助";

    public override string Summary => "获取帮助";

    public override string Description => """
                                          /help [command]
                                          获取命令的帮助信息。其中“command”为命令名，为空时返回可用命令列表。
                                          在线文档：https://docs.ziyuebot.cn/general/help
                                          """;

    public override Platform[] SupportedPlatform => [Platform.Discord, Platform.QQ];

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        Logger.Info(
            $"平台：${context.Platform}，调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);

        if (!arg.IsEmpty())
        {
            Command? command = Commands.GetCommand(context.Platform, arg.ToString(context));
            await context.SendMessage(command is not null ? command.Description : $"命令未找到：{arg.ToString(context)}");
            return;
        }

        string help = Commands.RegisteredCommands.Values.ToHashSet()
            .Where(command => command.SupportedPlatform.Contains(context.Platform)).Aggregate("子悦机器可用命令：\n",
                (current, command) => current + $"\t/{command.Id}\t{command.Name}\n");

        help += "输入“/help [命令名]”可以查看命令帮助。\n详细信息请查看在线文档：https://docs.ziyuebot.cn/";
        await context.SendMessage(help);
    }
}