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

    private static string FlattenTextMessage(GroupMessageEvent groupMessageEvent)
    {
        string result = "";
        foreach (IMessageEntity message in groupMessageEvent.Chain)
        {
            switch (message)
            {
                case TextEntity text:
                    result += text.Text;
                    break;
                case MentionEntity mention:
                    result += mention.Name[1..];
                    break;
            }
        }

        return result;
    }

    public static void Initialize()
    {
        ZiYueBot.Instance.QQ.Invoker.OnGroupMessageReceived += EventOnGroupMessageReceived;
        ZiYueBot.Instance.QQ.Invoker.OnFriendMessageReceived += EventOnFriendMessageReceived;
    }

    private static void EventOnFriendMessageReceived(BotContext context, FriendMessageEvent e)
    {
        if (context.BotUin == e.Chain.FriendUin) return;
        MessageBuilder builder = MessageBuilder.Friend(e.Chain.FriendUin);
        foreach (var item in e.Chain)
        {
            switch (item)
            {
                case FaceEntity face:
                    builder.Face(face.FaceId, face.IsLargeFace);
                    break;
                case ImageEntity image:
                    WebUtils.DownloadFile(image.ImageUrl, $"temp/{image.FilePath}").GetAwaiter().GetResult();
                    builder.Image($"temp/{image.FilePath}");
                    //Task.Run(() => { Thread.Sleep(10000); File.Delete($"temp/{image.FilePath}"); });
                    break;
                case MentionEntity mention:
                    builder.Mention(mention.Uin, mention.Name);
                    break;
                case TextEntity text:
                    builder.Text(text.Text);
                    break;
            }
        }

        context.SendMessage(builder.Build());
    }

    private static void EventOnGroupMessageReceived(BotContext context, GroupMessageEvent e)
    {
        try
        {
            if (context.BotUin == e.Chain.FriendUin) return;
            string userName = e.Chain.GroupMemberInfo.MemberName;
            uint userId = e.Chain.FriendUin;
            string[] args = Parser.Parse(FlattenTextMessage(e));
            if (e.Chain.First() is ImageEntity image && PicFace.Users.Contains(userId))
            {
                WebUtils.DownloadFile(image.ImageUrl, $"temp/{image.FilePath}").GetAwaiter().GetResult();
                context.SendMessage(
                    MessageBuilder.Group((uint)e.Chain.GroupUin).Image($"temp/{image.FilePath}").Build());
                PicFace.Users.Remove(userId);
                PicFace.Logger.Info($"{userId} 的表情转图片已完成：{image.FilePath}");
                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    File.Delete($"temp/{image.FilePath}");
                });
                return;
            }

            switch (args[0])
            {
                default:
                    IHarmonyCommand? harmony = Commands.GetHarmonyCommand<IHarmonyCommand>(args[0]);
                    if (harmony is not null)
                    {
                        context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin)
                            .Text(harmony.Invoke(userName, userId, args)).Build());
                    }
                    else
                    {
                        IGeneralCommand? general = Commands.GetGeneralCommand<IGeneralCommand>(Platform.QQ, args[0]);
                        if (general is not null)
                        {
                            context.SendMessage(MessageBuilder.Group((uint)e.Chain.GroupUin)
                                .Text(general.QQInvoke(userName, userId, args)).Build());
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