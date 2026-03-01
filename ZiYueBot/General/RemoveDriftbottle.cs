using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class RemoveDriftbottle : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("删除云瓶");

    public override string Id => "删除云瓶";

    public override string Name => "删除云瓶";

    public override string Summary => "删除一个漂流云瓶";

    public override string Description => """
                                          /删除云瓶 [id]
                                          删除一个自己扔出的漂流云瓶。“id”是瓶子的数字编号。
                                          在线文档：https://docs.ziyuebot.cn/general/driftbottle/remove
                                          """;

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (arg.IsEmpty())
        {
            await context.SendMessage("参数数量不足。使用“/help 删除云瓶”查看命令用法。");
            return;
        }

        int id;
        try
        {
            id = int.Parse(arg.ToString());
        }
        catch (FormatException)
        {
            await context.SendMessage("请输入数字编号！");
            return;
        }
        catch (OverflowException)
        {
            await context.SendMessage("编号过大！");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);

        await using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        await using MySqlCommand select = new MySqlCommand(
            $"SELECT * FROM driftbottles WHERE pickable = true AND id = {id}",
            database);
        await using MySqlDataReader reader = select.ExecuteReader();
        if (!reader.Read())
        {
            await context.SendMessage("找不到瓶子！");
            return;
        }

        bool privileged = Privileged.HasPrivilege(context.UserId, Privilege.RemoveDriftbottle);

        if (reader.GetUInt64("userid") != context.UserId && !privileged)
        {
            await context.SendMessage("该瓶子不是由你扔出的！");
            return;
        }

        await reader.CloseAsync();
        await using MySqlCommand command = new MySqlCommand(
            $"UPDATE driftbottles SET pickable = false WHERE id = {id}",
            database);
        command.ExecuteNonQuery();
        await context.SendMessage((privileged ? "[提权] " : "") + $"{id} 号瓶子已删除！");
    }
}