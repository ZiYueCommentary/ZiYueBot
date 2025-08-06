using System.Text.Json.Nodes;
using Yunhu.Api;
using Yunhu.Chat.Request;
using Yunhu.Chat.Request.ChatContent;
using Yunhu.Chat.Response;
using Yunhu.Content;
using Yunhu.Event;
using Message = ZiYueBot.Core.Message;
using YunhuMessage = Yunhu.Api.Message;

namespace ZiYueBot.Yunhu;

public static class Parser
{
    public static Message FlattenMessage(CommandEventContext context)
    {
        Message message = new Message
        {
            Text = context.Message.Command.Name + " "
        };

        if (context.Message.ParentId != null)
        {
            message.HasForward = true;
            IReceiver receiver = context.Chat.ChatType switch
            {
                ChatType.Bot => new UserReceiver((UserId)context.Chat.ChatId),
                ChatType.Group => new GroupReceiver((GroupId)context.Chat.ChatId)
            };
            GetMessageResponse response =
                ZiYueBot.Instance.Yunhu.ApiProvider.Request<GetMessageRequest, GetMessageResponse>(
                    new GetMessageRequest(receiver, context.Message.ParentId.Value));
            switch (response.MessageResult.Content)
            {
                case TextContent text:
                    message.Text += text.Text.Replace("\u200B", string.Empty) + " ";
                    break;
                case ImageContent image:
                    message.Text += $"\u2402{image.Url}\u2403 ";
                    break;
            }
        }

        switch (context.Message.Content)
        {
            case TextContent text:
                message.Text += text.Text.Replace("\u200B", string.Empty);
                break;
            case ImageContent image:
                message.Text += $"\u2402{image.Url}\u2403";
                break;
        }

        return message;
    }

    public static void SendMessage(Chat chat, string message)
    {
        IReceiver receiver = chat.ChatType switch
        {
            ChatType.Bot => new UserReceiver((UserId)chat.ChatId),
            ChatType.Group => new GroupReceiver((GroupId)chat.ChatId)
        };
        ZiYueBot.Instance.Yunhu.ApiProvider.Request<ChatRequest, ChatResponse>(new ChatRequest(receiver,new TextChatContent(message)));
    }
}