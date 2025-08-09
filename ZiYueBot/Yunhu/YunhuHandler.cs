using Yunhu.Api;
using Yunhu.Event;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Harmony;
using EventType = ZiYueBot.Core.EventType;
using Message = ZiYueBot.Core.Message;

namespace ZiYueBot.Yunhu;

public class YunhuHandler
{
    public static void Initialize()
    {
        ZiYueBot.Instance.Yunhu.Start();
        ZiYueBot.Instance.Yunhu.InstructionMessageReceived += InstructionMessageHandler;
    }

    private static void InstructionMessageHandler(object? sender, CommandEventContext context)
    {
        Message flatten = Parser.FlattenMessage(context);
        if (flatten.Text == "/") return;
        string[] args = flatten.Parse();

        switch (args[0])
        {
            default:
            {
                //if (args[0].Contains("云瓶") && flatten.HasForward)
                //{

                //    await Parser.SendMessage(eventType, sourceUin, "使用云瓶命令时不可回复消息！");
                //    break;
                //}

                HarmonyCommand? harmony = Commands.GetHarmonyCommand<HarmonyCommand>(args[0]);
                if (harmony is not null)
                {
                    Parser.SendMessage(context.Chat,
                        harmony.Invoke(
                            context.Chat.ChatType == ChatType.Group ? EventType.GroupMessage : EventType.DirectMessage,
                            context.Sender.Nickname, (ulong)context.Sender.Id.Id, args));
                }
                else
                {
                    GeneralCommand? general =
                        Commands.GetGeneralCommand<GeneralCommand>(Platform.Yunhu, args[0]);
                    if (general is not null)
                    {
                        Parser.SendMessage(context.Chat,
                            general.YunhuInvoke(
                                context.Chat.ChatType == ChatType.Group ? EventType.GroupMessage : EventType.DirectMessage,
                                context.Sender.Nickname, (uint)context.Sender.Id.Id, args));
                    }
                }

                break;
            }
        }
    }
}