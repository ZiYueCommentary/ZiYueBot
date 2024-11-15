using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.Harmony;

public class RemoveDriftbottle : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("删除云瓶");

    public string GetCommandId()
    {
        return "删除云瓶";
    }

    public string GetCommandName()
    {
        return "删除云瓶";
    }

    public string GetCommandDescription()
    {
        return """
               /删除云瓶 [id]
               删除一个自己扔出的漂流云瓶。“id”是瓶子的数字编号。
               在线文档：https://docs.ziyuebot.cn/remove-driftbottle.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "删除一个漂流云瓶";
    }

    public string Invoke(EventType type, string userName, ulong userId, string[] args)
    {
        if (args.Length < 2) return "参数数量不足。使用 “/help 删除云瓶” 查看命令用法。";
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
        using MySqlCommand select = new MySqlCommand(
            $"SELECT * FROM driftbottles WHERE pickable = true AND id = {id}",
            ZiYueBot.Instance.Database);
        using MySqlDataReader reader = select.ExecuteReader();
        if (!reader.Read()) return "找不到瓶子！";
        if (reader.GetUInt64("userId") != userId) return "该瓶子不是由你扔出的！";
        reader.Close();
        using MySqlCommand command = new MySqlCommand(
            $"UPDATE driftbottles SET pickable = false WHERE id = {id}",
            ZiYueBot.Instance.Database);
        command.ExecuteNonQuery();
        return $"{id} 号瓶子已删除！";
    }
}