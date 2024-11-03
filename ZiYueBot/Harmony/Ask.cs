using log4net;
using System.Text;

namespace ZiYueBot.Harmony;

public class Ask : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("评价");
    private readonly List<string> _reviews = [];

    public Ask()
    {
        using FileStream stream = new FileStream("data/words.txt", FileMode.OpenOrCreate);
        using StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(936));
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            _reviews.Add(line);
        }
        Logger.Info("张维为语录库加载完毕");
    }

    public string GetCommandDescription()
    {
        return """
/ask [question]
随机张维为教授语录。“question”为可选参数，输入后可以让张教授对指定问题做出评价。
在线文档：https://docs.ziyuebot.cn/ask.html
""";
    }

    public string GetCommandID()
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

    public string Invoke(string userName, ulong userId, string[] args)
    {
        Logger.Info($"调用者：{userName}（{userId}），参数：${string.Join(',')}");
        if (args.Length >= 2 && args[1] != "")
        {
            return $"张教授对 {args[1]} 的评价是：{_reviews[Random.Shared.Next(0, _reviews.Count - 1)]}";
        } else
        {
            return $"张教授的评价是：{_reviews[Random.Shared.Next(0, _reviews.Count - 1)]}";
        }
    }
}
