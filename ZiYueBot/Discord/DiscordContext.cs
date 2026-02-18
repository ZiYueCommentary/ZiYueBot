using Discord;
using Discord.WebSocket;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.Discord;

public class DiscordContext(EventType eventType, string userName, ulong userId, SocketSlashCommand socket) : IContext
{
    public override Platform Platform => Platform.Discord;
    public override EventType EventType { get; } = eventType;
    public override string UserName { get; } = userName;
    public override ulong UserId { get; } = userId;
    public SocketSlashCommand Socket { get; } = socket;

    /// 由于 Discord 默认回复调用消息，所以第一条需要采取回复，之后的消息必须以普通消息的形式发送。
    private bool _firstInvoke = true;

    public override async Task SendMessage(MessageChain messageChain)
    {
        IEnumerable<FileAttachment> images = messageChain.Where(message => message.Type == MessageEntityType.Image)
            .Select(message =>
            {
                ImageMessageEntity image = (ImageMessageEntity)message;
                return new FileAttachment(image.GetStreamAsync().GetAwaiter().GetResult(), image.FileName);
            });
        IEnumerable<string> text = messageChain.Where(message => message.Type != MessageEntityType.Image)
            .Select(message =>
            {
                return message switch
                {
                    TextMessageEntity text => text.Text,
                    PingMessageEntity ping =>
                        $" {Socket.Channel.GetUserAsync(ping.UserId).GetAwaiter().GetResult().Mention} ",
                    _ => throw new InvalidDataException()
                };
            });
        if (_firstInvoke)
        {
            await Socket.RespondWithFilesAsync(images, string.Join(null, text));
            _firstInvoke = false;
            return;
        }

        await Socket.Channel.SendFilesAsync(images, string.Join(null, text));
    }

    public override async Task<string> FetchUserName(ulong userId)
    {
        IUser? user = await Socket.Channel.GetUserAsync(userId);
        if (user is not null) return user.GlobalName;

        DiscordHandler.Logger.Warn($"用户信息获取失败：{userId}");
        return "[未知用户]";
    }
}