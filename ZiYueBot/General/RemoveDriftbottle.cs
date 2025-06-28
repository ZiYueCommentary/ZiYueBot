using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class RemoveDriftbottle : GeneralCommand
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

    public override Platform SupportedPlatform => Platform.Both;

    private string Invoke(ulong userId, int id)
    {
        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand select = new MySqlCommand(
            $"SELECT * FROM driftbottles WHERE pickable = true AND id = {id}",
            database);
        using MySqlDataReader reader = select.ExecuteReader();
        if (!reader.Read()) return "找不到瓶子！";
        if (reader.GetUInt64("userid") != userId) return "该瓶子不是由你扔出的！";
        reader.Close();
        using MySqlCommand command = new MySqlCommand(
            $"UPDATE driftbottles SET pickable = false WHERE id = {id}",
            database);
        command.ExecuteNonQuery();
        return $"{id} 号瓶子已删除！";
    }

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (args.Length < 2) return "参数数量不足。使用“/help 删除云瓶”查看命令用法。";
        int id = int.MinValue;
        try
        {
            id = int.Parse(args[1]);
        }
        catch (FormatException)
        {
            return "请输入数字编号！";
        }
        catch (OverflowException)
        {
            return "编号过大！";
        }

        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        UpdateInvokeRecords(userId);
        return Invoke(userId, id);
    }

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        int id = int.MinValue;
        try
        {
            id = int.Parse(args[0]);
        }
        catch (FormatException)
        {
            return "请输入数字编号！";
        }
        catch (OverflowException)
        {
            return "编号过大！";
        }

        Logger.Info($"调用者：{userPing} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        UpdateInvokeRecords(userId);
        return Invoke(userId, id);
    }
}