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
    /// <param name="userName">调用者的昵称</param>
    /// <param name="userId">调用者的ID</param>
    /// <param name="args">命令的参数。</param>
    /// <returns></returns>
    string Invoke(string userName, ulong userId, string[] args);
}
