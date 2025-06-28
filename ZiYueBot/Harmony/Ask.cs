using log4net;
using System.Text;
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
            using FileStream stream = new FileStream("resources/ziyue.txt", FileMode.OpenOrCreate);
            using StreamReader reader = new StreamReader(stream);
            AprilReviews.Add(("子悦", []));
            while (reader.ReadLine() is { } line)
            {
                AprilReviews.Last().Item2.Add(line);
            }

            Logger.Info("愚人节语录库（子悦）加载完毕");
        }
        catch (Exception ex)
        {
            Logger.Error("愚人节语录库（子悦）加载失败！", ex);
        }

        try
        {
            using FileStream stream = new FileStream("resources/easytt.txt", FileMode.OpenOrCreate);
            using StreamReader reader = new StreamReader(stream);
            AprilReviews.Add(("义贼哥", []));
            while (reader.ReadLine() is { } line)
            {
                AprilReviews.Last().Item2.Add(line);
            }

            Logger.Info("愚人节语录库（义贼哥）加载完毕");
        }
        catch (Exception ex)
        {
            Logger.Error("愚人节语录库（义贼哥）加载失败！", ex);
        }

        try
        {
            using FileStream stream = new FileStream("resources/asriel.txt", FileMode.OpenOrCreate);
            using StreamReader reader = new StreamReader(stream);
            AprilReviews.Add(("山羊", []));
            while (reader.ReadLine() is { } line)
            {
                AprilReviews.Last().Item2.Add(line);
            }

            Logger.Info("愚人节语录库（山羊）加载完毕");
        }
        catch (Exception ex)
        {
            Logger.Error("愚人节语录库（山羊）加载失败！", ex);
        }

        try
        {
            using FileStream stream = new FileStream("resources/capybara.txt", FileMode.OpenOrCreate);
            using StreamReader reader = new StreamReader(stream);
            AprilReviews.Add(("水豚哥", []));
            while (reader.ReadLine() is { } line)
            {
                AprilReviews.Last().Item2.Add(line);
            }

            Logger.Info("愚人节语录库（水豚哥）加载完毕");
        }
        catch (Exception ex)
        {
            Logger.Error("愚人节语录库（水豚哥）加载失败！", ex);
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