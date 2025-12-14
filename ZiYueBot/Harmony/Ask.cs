using log4net;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.Harmony;

public class Ask : HarmonyCommand
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

    public override string Id => "ask";

    public override string Name => "评价";

    public override string Summary => "获取张维为教授语录";

    public override string Description => """
                                          /ask [question]
                                          随机张维为教授语录。“question”为可选参数，输入后可以让张教授对指定问题做出评价。
                                          在线文档：https://docs.ziyuebot.cn/harmony/ask
                                          """;

    public override string Invoke(EventType eventType, string userName, ulong userId, string[] args)
    {
        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        UpdateInvokeRecords(userId);

        if (args.Length >= 2)
        {
            string arguments = string.Join(' ', args[1..]);
            if (arguments != "")
                return $"张教授对 {arguments} 的评价是：{Reviews[Random.Shared.Next(0, Reviews.Count - 1)]}";
        }

        return $"张教授的评价是：{Reviews[Random.Shared.Next(0, Reviews.Count - 1)]}";
    }
}