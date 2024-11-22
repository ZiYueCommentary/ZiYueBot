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
    public delegate MessageBuilder MetaMessageBuilder();

    private static readonly ILog Logger = LogManager.GetLogger("QQ 消息解析");

    public static void Initialize()
    {
        ZiYueBot.Instance.QQ.Invoker.OnGroupMessageReceived += EventOnGroupMessageReceived;
        ZiYueBot.Instance.QQ.Invoker.OnFriendMessageReceived += EventOnFriendMessageReceived;
    }

    private static void EventHandler(BotContext context, EventType eventType, Message flatten, uint sender,
        string username,
        MetaMessageBuilder meta, IMessageEntity picfaceMessage)
    {
        try
        {
            if (context.BotUin == sender) return;
            if (flatten.Text == "/") return;
            string[] args = Parser.Parse(flatten.Text);
            if (picfaceMessage is ImageEntity image && PicFace.Users.Contains(sender))
            {
                context.SendMessage(meta().Image(WebUtils.DownloadFile(image.ImageUrl)).Text(image.ImageUrl).Build());
                PicFace.Users.Remove(sender);
                PicFace.Logger.Info($"{username} 的表情转图片已完成：{image.ImageUrl}");
                return;
            }

            switch (args[0])
            {
                case "xibao":
                {
                    Xibao xibao = Commands.GetHarmonyCommand<Xibao>();
                    string result = xibao.Invoke(eventType, username, sender, args);
                    if (result != "")
                    {
                        context.SendMessage(meta().Text(result).Build());
                        break;
                    }

                    context.SendMessage(meta().Image(Xibao.Render(true, args[1])).Build());
                    break;
                }
                case "beibao":
                {
                    Beibao beibao = Commands.GetHarmonyCommand<Beibao>();
                    string result = beibao.Invoke(eventType, username, sender, args);
                    if (result != "")
                    {
                        context.SendMessage(meta().Text(result).Build());
                        break;
                    }

                    context.SendMessage(meta().Image(Xibao.Render(false, args[1])).Build());
                    break;
                }
                case "balogo":
                {
                    BALogo baLogo = Commands.GetHarmonyCommand<BALogo>();
                    string result = baLogo.Invoke(eventType, username, sender, args);
                    if (result != "")
                    {
                        context.SendMessage(meta().Text(result).Build());
                        break;
                    }

                    context.SendMessage(meta().Image(baLogo.Render(args[1], args[2])).Build());
                    break;
                }
                default:
                {
                    if (args[0].Contains("云瓶") && flatten.HasForward)
                    {
                        context.SendMessage(meta().Text("使用云瓶命令时不可回复消息！").Build());
                        break;
                    }

                    IHarmonyCommand? harmony = Commands.GetHarmonyCommand<IHarmonyCommand>(args[0]);
                    if (harmony is not null)
                    {
                        context.SendMessage(
                            Parser.HierarchizeMessage(meta, harmony.Invoke(eventType, username, sender, args)
                            ).Build());
                    }
                    else
                    {
                        IGeneralCommand? general = Commands.GetGeneralCommand<IGeneralCommand>(Platform.QQ, args[0]);
                        if (general is not null)
                        {
                            context.SendMessage(
                                Parser.HierarchizeMessage(meta, general.QQInvoke(eventType, username, sender, args)
                                ).Build());
                        }
                        else if (flatten.Text.StartsWith('/'))
                        {
                            context.SendMessage(meta().Text("未知命令。请使用 /help 查看命令列表。").Build());
                        }
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message, ex);
            context.SendMessage(meta().Text("命令解析错误。").Build());
        }
    }

    private static void EventOnGroupMessageReceived(BotContext context, GroupMessageEvent e)
    {
        context.RequestFriend(3198883438u);
        EventHandler(context, EventType.GroupMessage, Parser.FlattenMessage(context, e.Chain), e.Chain.FriendUin,
            e.Chain.GroupMemberInfo.MemberName, () => MessageBuilder.Group((uint)e.Chain.GroupUin), e.Chain.First());
    }
    
    private static void EventOnFriendMessageReceived(BotContext context, FriendMessageEvent e)
    {
        EventHandler(context, EventType.DirectMessage, Parser.FlattenMessage(context, e.Chain), e.Chain.FriendUin,
            e.Chain.FriendInfo.Nickname, () => MessageBuilder.Friend(e.Chain.FriendUin), e.Chain.First());
        context.FetchFriends(true);
    }
}