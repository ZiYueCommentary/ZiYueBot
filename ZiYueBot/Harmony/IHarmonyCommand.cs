using ZiYueBot.Core;

namespace ZiYueBot.Harmony;

/// <summary>
/// 鸿蒙命令。
/// </summary>
public interface IHarmonyCommand : ICommand
{
    /// <summary>
    /// 调用鸿蒙命令。
    /// </summary>
    /// <param name="eventType">消息来源</param>
    /// <param name="userName">调用者的昵称</param>
    /// <param name="userId">调用者的ID</param>
    /// <param name="args">命令的参数</param>
    /// <returns>要发送的内容</returns>
    string Invoke(EventType eventType, string userName, ulong userId, string[] args);
}