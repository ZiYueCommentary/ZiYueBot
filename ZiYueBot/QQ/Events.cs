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
        ZiYueBot.Instance.QQ.Invoker.OnTempMessageReceived += EventOnTempMessageReceived;
    }

    /// <summary>
    /// 处理 QQ 消息。
    /// </summary>
    /// <param name="context">机器人本体</param>
    /// <param name="eventType">消息来源</param>
    /// <param name="message">消息内容</param>
    /// <param name="userId">来源用户 ID</param>
    /// <param name="userName">来源用户昵称</param>
    /// <param name="meta">消息构建元，用于构建消息</param>
    /// <param name="sourceUin">消息所在群 ID 或好友 ID</param>
    private static void EventHandler(BotContext context, EventType eventType, MessageChain message, uint userId,
        string userName, MetaMessageBuilder meta, ulong sourceUin)
    {
        try
        {
            if (context.BotUin == userId) return;
            Message flatten = Parser.FlattenMessage(context, message);
            if (flatten.Text == "/") return;
            string[] args = Parser.Parse(flatten.Text);
            if (message.First() is ImageEntity image && PicFace.Users.Contains(userId))
            {
                context.SendMessage(meta().Image(WebUtils.DownloadFile(image.ImageUrl)).Text(image.ImageUrl).Build());
                PicFace.Users.Remove(userId);
                PicFace.Logger.Info($"{userName} 的表情转图片已完成：{image.ImageUrl}");
                return;
            }

            switch (args[0])
            {
                case "开始俄罗斯轮盘":
                {
                    StartRevolver startRevolver = Commands.GetHarmonyCommand<StartRevolver>();
                    args[0] = sourceUin.ToString(); // 群聊 ID
                    context.SendMessage(meta().Text(startRevolver.Invoke(eventType, userName, userId, args)).Build());
                    break;
                }
                case "重置俄罗斯轮盘":
                {
                    RestartRevolver startRevolver = Commands.GetHarmonyCommand<RestartRevolver>();
                    args[0] = sourceUin.ToString(); // 群聊 ID
                    context.SendMessage(meta().Text(startRevolver.Invoke(eventType, userName, userId, args)).Build());
                    break;
                }
                case "开枪":
                {
                    Shooting shooting = Commands.GetHarmonyCommand<Shooting>();
                    args[0] = sourceUin.ToString(); // 群聊 ID
                    if (args.Length < 2)
                    {
                        List<string> list = args.ToList();
                        list.Add($"\u2404{userId}\u2405");
                        Message.MentionedUinAndName[userId] = $"@{userName}";
                        args = list.ToArray();
                    }

                    if (args[1].StartsWith('\u2404') && args[1].EndsWith('\u2405'))
                    {
                        args[1] = args[1][1..^1];
                        context.SendMessage(meta().Text(shooting.Invoke(eventType, userName, userId, args)).Build());
                        break;
                    }

                    context.SendMessage(meta().Text("参数无效。使用“/help 开枪”查看命令用法。").Build());

                    break;
                }
                case "转轮":
                {
                    Rotating rotating = Commands.GetHarmonyCommand<Rotating>();
                    args[0] = sourceUin.ToString(); // 群聊 ID
                    context.SendMessage(meta().Text(rotating.Invoke(eventType, userName, userId, args)).Build());
                    break;
                }
                case "xibao":
                {
                    Xibao xibao = Commands.GetHarmonyCommand<Xibao>();
                    string result = xibao.Invoke(eventType, userName, userId, args);
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
                    string result = beibao.Invoke(eventType, userName, userId, args);
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
                    string result = baLogo.Invoke(eventType, userName, userId, args);
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
                            Parser.HierarchizeMessage(meta, harmony.Invoke(eventType, userName, userId, args)
                            ).Build());
                    }
                    else
                    {
                        IGeneralCommand? general = Commands.GetGeneralCommand<IGeneralCommand>(Platform.QQ, args[0]);
                        if (general is not null)
                        {
                            context.SendMessage(
                                Parser.HierarchizeMessage(meta, general.QQInvoke(eventType, userName, userId, args)
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
        EventHandler(context, EventType.GroupMessage, e.Chain,
            e.Chain.FriendUin, e.Chain.GroupMemberInfo.MemberName, () => MessageBuilder.Group((uint)e.Chain.GroupUin),
            (uint)e.Chain.GroupUin);
    }

    private static void EventOnFriendMessageReceived(BotContext context, FriendMessageEvent e)
    {
        EventHandler(context, EventType.DirectMessage, e.Chain,
            e.Chain.FriendUin, e.Chain.FriendInfo.Nickname, () => MessageBuilder.Friend(e.Chain.FriendUin),
            0);
        context.FetchFriends(true); // 刷新好友列表
    }

    private static void EventOnTempMessageReceived(BotContext context, TempMessageEvent e)
    {
        context.RequestFriend(e.Chain.FriendUin); // 我也不知道这个能不能工作
    }
}