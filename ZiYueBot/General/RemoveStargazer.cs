using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class RemoveStargazer : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("删除星标");

    public override string Id => "删除星标";
    public override string Name => "删除星标";
    public override string Summary => "删除星标";
    public override string Description => "";

    public override async Task Invoke(Context context, MessageChain arg)
    {
        if (arg.IsEmpty())
        {
            await context.SendMessage("参数数量不足。使用“/help 删除星标”查看命令用法。");
            return;
        }

        if (arg[0] is not TextMessageEntity text || !int.TryParse(text.Text, out int id))
        {
            await context.SendMessage("参数无效。使用“/help 删除星标”查看命令用法。");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})");
        _ = UpdateInvokeRecords(context.UserId);

        await using MySqlCommand command = new MySqlCommand(
            $"UPDATE stargazers SET removed = true WHERE userid = {context.UserId} AND bottle_id = {id}",
            ZiYueBot.Instance.ConnectDatabase());
        await context.SendMessage($"{id} 号云瓶的星标已删除！");
    }
}