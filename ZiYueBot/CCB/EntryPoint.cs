using CCB.Abstractions;
using CCB.Attributes;
using CCB.Internal;
using ZiYueBot.Core;

namespace ZiYueBot.CCB;

[Injectable]
public class EntryPoint : ILoad, IUnload
{
    public void Load()
    {
        Program.Start();
        EventRegistry.PlayerChat += EventHandler;
    }

    public void Unload()
    {
        EventRegistry.PlayerChat -= EventHandler;
    }

    private static void EventHandler(EventRegistry.PlayerChatEventArg eventArg)
    {
        string[] args = eventArg.Text.Split(' ', 2);
        bool explicitInvoke = args[0].StartsWith('/');
        Command? command = Commands.GetCommand(Platform.CBMR, args[0].TrimStart('/'));
        if (command is null)
        {
            if (Commands.CheckAlias(args[0].TrimStart('/'), out string prompt))
            {
                GlobalProperties.Chat.SendPlayer(eventArg.Player, $"&colr[0 255 255]命令未找到，你是否在找 /{prompt}？");
                eventArg.EventResult = false;
                return;
            }

            if (explicitInvoke)
            {
                GlobalProperties.Chat.SendPlayer(eventArg.Player, "&colr[255 0 0]未知命令。请使用 /help 查看命令列表。");
            }
            eventArg.EventResult = !explicitInvoke;
            return;
        }

        CcbContext context = new CcbContext(eventArg.Player.GetName(), eventArg.Player.GetSteamID());
        context.SendMessage($"&colr[255 215 0]{context.UserName} 调用了子悦机器 /{args[0].TrimStart('/')}");
        command.Invoke(context, args.Length > 1 ? [new TextMessageEntity(args[1])] : []).GetAwaiter().GetResult();
        eventArg.EventResult = false;
    }
}