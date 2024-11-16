using log4net;
using ZiYueBot.Core;

namespace ZiYueBot.Harmony;

public class Quotations : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("毛主席语录");
    private static readonly List<string> Quotes = [];
    
    static Quotations()
    {
        try
        {
            using FileStream stream = new FileStream("resources/quotations.txt", FileMode.OpenOrCreate);
            using StreamReader reader = new StreamReader(stream);
            string full = reader.ReadToEnd();
            int pos = 0;
            for (int i = 0; i < full.Length; i++)
            {
                if (full[i] != '~') continue;
                Quotes.Add(full[pos..(i - 2)]);
                pos = i + 3;
            }
            Quotes.Add(full[pos..]);

            Logger.Info("毛主席语录加载完毕");
        }
        catch (Exception ex)
        {
            Logger.Error("毛主席语录加载失败！", ex);
        }
    }
    
    public string GetCommandId()
    {
        return "quotations";
    }

    public string GetCommandName()
    {
        return "毛主席语录";
    }

    public string GetCommandDescription()
    {
        return """
               /quotations
               从《毛主席语录》中随机获取一句毛泽东主席语录。
               在线文档：https://docs.ziyuebot.cn/quotations.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "随机一句毛泽东主席语录";
    }

    public string Invoke(EventType eventType, string userName, ulong userId, string[] args)
    {
        Logger.Info($"调用者：{userName} ({userId})");
        return Quotes[Random.Shared.Next(0, Quotes.Count - 1)];
    }
}