using log4net;
using ZiYueBot.Core;

namespace ZiYueBot.Harmony;

public class Hitokoto : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("一言");

    public string GetCommandDescription()
    {
        return """
               /hitokoto
               获得一句话。
               在线文档：https://docs.ziyuebot.cn/hitokoto.html
               """;
    }

    public string GetCommandId()
    {
        return "hitokoto";
    }

    public string GetCommandName()
    {
        return "一言";
    }

    public string GetCommandShortDescription()
    {
        return "随机一句话";
    }

    public string Invoke(EventType type, string userName, ulong userId, string[] args)
    {
        Logger.Info($"调用者：{userName} ({userId})");
        using HttpClient client = new HttpClient();
        try
        {
            HttpResponseMessage response = client.GetAsync("https://v1.hitokoto.cn/?c=f&encode=text").Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
        }
        catch (HttpRequestException e)
        {
            Logger.Error(e.Message, e);
        }

        return "一言获取失败。";
    }
}