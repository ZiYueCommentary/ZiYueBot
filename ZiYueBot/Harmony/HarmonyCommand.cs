using System.Collections;
using ZiYueBot.Core;
using ZiYueBot.General;

namespace ZiYueBot.Harmony;

/// <summary>
/// 鸿蒙命令。
/// </summary>
public abstract class HarmonyCommand : GeneralCommand
{
    public sealed override Platform SupportedPlatform => Platform.Both;

    public override IEnumerable Invoke(Platform platform, EventType eventType, string userName, ulong userId,
        string[] args)
    {
        return Invoke(eventType, userName, userId, args);
    }

    /// <summary>
    /// 调用鸿蒙命令。
    /// </summary>
    /// <param name="eventType">消息来源</param>
    /// <param name="userName">调用者的昵称</param>
    /// <param name="userId">调用者的ID</param>
    /// <param name="args">命令的参数</param>
    /// <returns>要发送的内容</returns>
    public abstract IEnumerable Invoke(EventType eventType, string userName, ulong userId, string[] args);
}