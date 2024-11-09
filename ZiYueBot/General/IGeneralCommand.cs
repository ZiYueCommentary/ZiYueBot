using ZiYueBot.Core;

namespace ZiYueBot.General;

public interface IGeneralCommand : ICommand
{
    Platform GetSupportedPlatform();
    string QQInvoke(EventType eventType, string userName, uint userId, string[] args);
    string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args);
}