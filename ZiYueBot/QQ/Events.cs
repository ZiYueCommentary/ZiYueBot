using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message.Entity;
using Lagrange.Core.Message;
using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using log4net;
using ZiYueBot.Harmony;
using ZiYueBot.Core;
using ZiYueBot.General;

namespace ZiYueBot.QQ;

public static class Events
{
    private static readonly ILog LoggerGroup = LogManager.GetLogger("QQ 消息解析");

    public static void Initialize()
    {
        ZiYueBot.Instance.QQ.Invoker.OnGroupMessageReceived += EventOnGroupMessageReceived;
    }

    private static void EventOnGroupMessageReceived(BotContext context, GroupMessageEvent e)
    {
        try
        {
            if (context.BotUin == e.Chain.FriendUin) return;
            string userName = e.Chain.GroupMemberInfo.MemberName;
            uint userId = e.Chain.FriendUin;
            string flatten = Parser.FlattenMessage(context, e.Chain);
            if (flatten == "/") return;
            string[] args = Parser.Parse(flatten);
            if (e.Chain.First() is ImageEntity image && PicFace.Users.Contains(userId))
            {
                context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin)
                    .Image(WebUtils.DownloadFile(image.ImageUrl)).Text(image.ImageUrl).Build());
                PicFace.Users.Remove(userId);
                PicFace.Logger.Info($"{userId} 的表情转图片已完成：{image.ImageUrl}");
                return;
            }

            switch (args[0])
            {
                case "xibao":
                {
                    Xibao xibao = Commands.GetHarmonyCommand<Xibao>();
                    string result = xibao.Invoke(EventType.GroupMessage, userName, userId, args);
                    if (result != "")
                    {
                        context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin).Text(result).Build());
                        break;
                    }
                    context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin).Image(Xibao.Render(true, args[1]))
                        .Build());
                    break;
                }
                case "beibao":
                {
                    Beibao beibao = Commands.GetHarmonyCommand<Beibao>();
                    string result = beibao.Invoke(EventType.GroupMessage, userName, userId, args);
                    if (result != "")
                    {
                        context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin).Text(result).Build());
                        break;
                    }

                    context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin).Image(Xibao.Render(false, args[1]))
                        .Build());
                    break;
                }
                case "balogo":
                {
                    BALogo baLogo = Commands.GetHarmonyCommand<BALogo>();
                    string result = baLogo.Invoke(EventType.GroupMessage, userName, userId, args);
                    if (result != "")
                    {
                        context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin).Text(result).Build());
                        break;
                    }

                    context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin)
                        .Image(baLogo.Render(args[1], args[2])).Build());
                    break;
                }
                default:
                {
                    IHarmonyCommand? harmony = Commands.GetHarmonyCommand<IHarmonyCommand>(args[0]);
                    if (harmony is not null)
                    {
                        context.SendMessage(Parser.HierarchizeMessage((uint)e.Chain.GroupUin,
                            harmony.Invoke(EventType.GroupMessage, userName, userId, args)).Build());
                    }
                    else
                    {
                        IGeneralCommand? general = Commands.GetGeneralCommand<IGeneralCommand>(Platform.QQ, args[0]);
                        if (general is not null)
                        {
                            context.SendMessage(Parser.HierarchizeMessage((uint)e.Chain.GroupUin,
                                general.QQInvoke(EventType.GroupMessage, userName, userId, args)).Build());
                        }
                        else if (flatten.StartsWith('/'))
                        {
                            context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin)
                                .Text("未知命令。请使用 /help 查看命令列表。")
                                .Build());
                        }
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            LoggerGroup.Error(ex.Message, ex);
            context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin).Text("命令解析错误。").Build());
        }
    }
}