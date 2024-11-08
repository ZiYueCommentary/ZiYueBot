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

    /// <summary>
    /// 扁平化 QQ 消息，以便于传输给各命令。
    /// </summary>
    private static string FlattenMessage(BotContext context, MessageChain chain, bool ignoreForward = false)
    {
        string result = "";
        string forwardMessage = ""; // 被引用的消息内容
        bool hasForwardMessage = false;
        bool wasMention = false; // 如果上一个消息是提及，则删除一个空格，以便把前后消息看成一个整体。
        for (int i = 0; i < chain.Count; i++)
        {
            // 除纯文本外，其他类型的特殊消息将被控制字符包裹，以便于发送时层级化。
            switch (chain[i])
            {
                case ForwardEntity forward:
                    if (ignoreForward)
                    {
                        result = "";
                        continue;
                    }

                    forwardMessage = FlattenMessage(context,
                        context.GetGroupMessage((uint)chain.GroupUin, forward.Sequence, forward.Sequence)
                            .GetAwaiter().GetResult().First());
                    result = "";
                    hasForwardMessage = true;
                    wasMention = false;
                    break;
                case TextEntity text:
                    result += text.Text[(wasMention && text.Text.StartsWith(' ') ? 1 : 0) ..];
                    wasMention = false;
                    break;
                case ImageEntity image:
                    result += $"\u2402{image.ImageUrl}\u2403";
                    wasMention = false;
                    break;
                case MentionEntity mention:
                    if (ignoreForward && i == 0) continue;
                    result += $"\u2404{mention.Uin}\u2405";
                    wasMention = true;
                    break;
                case FaceEntity face:
                    result += $"\u2406{face.FaceId}\u2407";
                    wasMention = false;
                    break;
            }
        }

        if (!hasForwardMessage) return result;

        return result.Contains(' ')
            ? result.Insert(result.IndexOf(' '), $" \"{forwardMessage}\" ")
            : $"{result} \"{forwardMessage}\"";
    }

    private static MessageBuilder HierarchizeMessage(uint groupUin, string message)
    {
        bool simpleMessage = true;
        MessageBuilder builder = MessageBuilder.Group(groupUin);
        int pos = 0;
        for (int i = 0; i < message.Length; i++)
        {
            switch (message[i])
            {
                case '\u2402': // 图片
                {
                    builder.Text(message.Substring(pos, i - pos - (pos == 0 ? 0 : 1)));
                    int end = message.IndexOf('\u2403', i + 1);
                    builder.Image(WebUtils.DownloadFile(message.Substring(i + 1, end - i - 1)));
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
                case '\u2404': // 提及
                {
                    builder.Text(message.Substring(pos, i - pos - (pos == 0 ? 0 : 1)));
                    int end = message.IndexOf('\u2405', i + 1);
                    builder.Mention(uint.Parse(message.Substring(i + 1, end - i - 1))).Text(" "); // 提及后面必须加空格，否则会显示出错。
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
                case '\u2406': // 表情
                {
                    builder.Text(message.Substring(pos, i - pos - (pos == 0 ? 0 : 1)));
                    int end = message.IndexOf('\u2407', i + 1);
                    builder.Face(ushort.Parse(message.Substring(i + 1, end - i - 1)));
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
            }
        }

        if (simpleMessage) return builder.Text(message);
        
        if (pos < message.Length) builder.Text(message[(pos + (message[pos + 1] == ' ' ? 2 : 1))..]);
        return builder;
    }

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
            string flatten = FlattenMessage(context, e.Chain);
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
                default:
                    IHarmonyCommand? harmony = Commands.GetHarmonyCommand<IHarmonyCommand>(args[0]);
                    if (harmony is not null)
                    {
                        context.SendMessage(HierarchizeMessage((uint)e.Chain.GroupUin,
                            harmony.Invoke(userName, userId, args)).Build());
                    }
                    else
                    {
                        IGeneralCommand? general = Commands.GetGeneralCommand<IGeneralCommand>(Platform.QQ, args[0]);
                        if (general is not null)
                        {
                            context.SendMessage(HierarchizeMessage((uint)e.Chain.GroupUin,
                                general.QQInvoke(userName, userId, args)).Build());
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
        catch (Exception ex)
        {
            LoggerGroup.Error(ex.Message, ex);
            context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin).Text("命令解析错误。").Build());
        }
    }
}