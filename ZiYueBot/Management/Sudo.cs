using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.Management;

public class Sudo : Command
{
    public override string Id => "sudo";
    public override string Name => "提权";
    public override string Summary => "管理提权";
    public override string Description => "";
    public override Platform[] SupportedPlatform => [Platform.Management];

    public override async Task Invoke(Context context, MessageChain arg)
    {
        if (arg.IsEmpty() || arg[0] is not TextMessageEntity text)
        {
            _ = WriteLog(context.UserId, arg.Flatten(), Result.Invalid);
            await context.SendMessage("参数不足或无效。");
            return;
        }

        string[] split = text.Text.Split(' ', 2);
        Command? command = Commands.GetCommand(Platform.Management, split[0]);
        if (command is not PrivilegeCommand privilegeCommand)
        {
            _ = WriteLog(context.UserId, arg.Flatten(), Result.Invalid);
            await context.SendMessage("找不到命令或命令无需提权。");
            return;
        }

        if (!Privileged.HasPrivilege(context.UserId, privilegeCommand.ExpectingPrivileges))
        {
            _ = WriteLog(context.UserId, arg.Flatten(), Result.Failed);
            await context.SendMessage($"权限不足，需要 {privilegeCommand.ExpectingPrivileges:F} 特权。");
            return;
        }

        MessageChain argChain = [];
        if (split.Length > 1 && !string.IsNullOrEmpty(split[1])) argChain.Add(new TextMessageEntity(split[1]));
        if (arg.Count > 1) argChain.AddRange(arg[1..]);
        _ = WriteLog(context.UserId, arg.Flatten(), Result.Success);
        await privilegeCommand.PrivilegedInvoke(context, argChain);
    }

    protected static async Task WriteLog(ulong userId, string command, Result result)
    {
        await using MySqlCommand query = new MySqlCommand(
            $"INSERT INTO sudo_logs VALUE(now(), {userId}, @command, '{result:G}')",
            ZiYueBot.Instance.ConnectDatabase());
        query.Parameters.AddWithValue("@command", command);
        await query.ExecuteNonQueryAsync();
    }

    protected enum Result
    {
        Success,
        Failed,
        Invalid
    }
}