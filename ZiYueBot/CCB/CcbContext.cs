using CCB.Internal;
using ZiYueBot.Core;

namespace ZiYueBot.CCB;

public class CcbContext(string userName, string userId) : Context
{
    public override Platform Platform => Platform.CBMR;
    public override EventType EventType => EventType.GroupMessage;
    public override string UserName { get; } = userName;
    public override string UserId { get; } = userId;

    public override Task SendMessage(MessageChain messageChain)
    {
        foreach (string line in messageChain.ToString(this).Split('\n'))
        {
            GlobalProperties.Chat.Send($"&colr[80 220 255]{line}");
        }
        return Task.CompletedTask;
    }
}