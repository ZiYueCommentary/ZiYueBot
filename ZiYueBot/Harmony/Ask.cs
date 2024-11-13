using log4net;
using System.Text;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.Harmony;

public class Ask : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("评价");
    private static readonly List<string> Reviews = [];

    static Ask()
    {
        try
        {
            using FileStream stream = new FileStream("resources/words.txt", FileMode.OpenOrCreate);
            using StreamReader reader = new StreamReader(stream);
            while (reader.ReadLine() is { } line)
            {
                Reviews.Add(line);
            }

            Logger.Info("张维为语录库加载完毕");
        }
        catch (Exception ex)
        {
            Logger.Error("张维为语录库加载失败！", ex);
        }
    }

    public string GetCommandDescription()
    {
        return """
               /ask [question]
               随机张维为教授语录。“question”为可选参数，输入后可以让张教授对指定问题做出评价。
               在线文档：https://docs.ziyuebot.cn/ask.html
               """;
    }

    public string GetCommandId()
    {
        return "ask";
    }

    public string GetCommandName()
    {
        return "评价";
    }

    public string GetCommandShortDescription()
    {
        return "获取张维为教授语录";
    }

    public string Invoke(EventType type, string userName, ulong userId, string[] args)
    {
        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        if (args.Length >= 2 && args[1] != "")
        {
            return $"张教授对 {args[1]} 的评价是：{Reviews[Random.Shared.Next(0, Reviews.Count - 1)]}";
        }

        return $"张教授的评价是：{Reviews[Random.Shared.Next(0, Reviews.Count - 1)]}";
    }
}