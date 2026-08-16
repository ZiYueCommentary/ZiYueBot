using System.Text.Json;
using ZiYueBot.Core;

namespace ZiYueBot.Harmony;

public class Ask : Command
{
    // private static readonly ILog Logger = LogManager.GetLogger("评价");
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

            // Logger.Info("张维为语录库加载完毕");
        }
        catch (Exception ex)
        {
            // Logger.Error("张维为语录库加载失败！", ex);
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

    public override async Task Invoke(Context context, MessageChain arg)
    {
        // Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);

        if (arg.IsEmpty())
            await context.SendMessage($"张教授的评价是：{Reviews[Random.Shared.Next(0, Reviews.Count - 1)]}");
        else
            await context.SendMessage("张教授对 " + arg + $" 的评价是：{Reviews[Random.Shared.Next(0, Reviews.Count - 1)]}");
    }
}