using ZiYueBot.Core;

namespace ZiYueBot.Management;

public abstract class PrivilegeCommand : Command
{
    public abstract Privilege[] ExpectingPrivileges { get; }
    public override Platform[] SupportedPlatform => [Platform.Management];

    public sealed override async Task Invoke(Context context, MessageChain arg)
    {
        await context.SendMessage("权限不足，你有使用 sudo 吗？");
    }

    public abstract Task PrivilegedInvoke(Context context, MessageChain arg);
}