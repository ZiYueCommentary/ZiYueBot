using ZiYueBot.Core;

namespace ZiYueBot.General;

public interface IGeneralCommand : ICommand
{
    Platform GetSupportedPlatform();
    string QQInvoke(string userName, uint userId, string[] args);
    string DiscordInvoke(string userPing, ulong userId, string[] args);
}