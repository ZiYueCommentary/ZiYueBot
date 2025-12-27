using log4net;
using System.Text;
using System.Text.Json;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.Harmony;

public class Ask : HarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("评价");
    private static readonly List<string> Reviews = [];
    private static readonly List<(string, List<string>)> AprilReviews = [];

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

        try
        {
            using FileStream stream = new FileStream("resources/april_words.json", FileMode.OpenOrCreate);
            using StreamReader reader = new StreamReader(stream);
            string jsonContent = reader.ReadToEnd();

            if (!string.IsNullOrEmpty(jsonContent))
            {
                Dictionary<string, List<string>>? aprilWords =
                    JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent);

                if (aprilWords != null)
                    foreach (KeyValuePair<string, List<string>> person in aprilWords)
                    {
                        AprilReviews.Add((person.Key, [.. person.Value]));
                    }
            }

            Logger.Info($"愚人节语录库加载完毕，共 {AprilReviews.Count} 人");
        }
        catch (Exception ex)
        {
            Logger.Error("愚人节语录库加载失败！", ex);
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

        if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1)
        {
            (string, List<string>) aprilReview = AprilReviews[Random.Shared.Next(0, AprilReviews.Count)];
            if (args.Length >= 2)
            {
                string arguments = string.Join(' ', args[1..]);
                if (arguments != "")
                    return
                        $"{aprilReview.Item1}对 {arguments} 的评价是：{aprilReview.Item2[Random.Shared.Next(0, aprilReview.Item2.Count - 1)]}";
            }

            return
                $"{aprilReview.Item1}的评价是：{aprilReview.Item2[Random.Shared.Next(0, aprilReview.Item2.Count - 1)]}";
        }

        if (args.Length >= 2)
        {
            string arguments = string.Join(' ', args[1..]);
            if (arguments != "")
                return $"张教授对 {arguments} 的评价是：{Reviews[Random.Shared.Next(0, Reviews.Count - 1)]}";
        }

        return $"张教授的评价是：{Reviews[Random.Shared.Next(0, Reviews.Count - 1)]}";
    }
}