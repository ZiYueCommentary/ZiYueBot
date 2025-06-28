using log4net;
using ZiYueBot.Core;

namespace ZiYueBot.Harmony;

public class Hitokoto : HarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("一言");

    public override string Id => "hitokoto";

    public override string Name => "一言";

    public override string Summary => "随机一句话";

    public override string Description => """
                                          /hitokoto
                                          获得一句话。
                                          在线文档：https://docs.ziyuebot.cn/harmony/hitokoto
                                          """;

    public override string Invoke(EventType eventType, string userName, ulong userId, string[] args)
    {
        Logger.Info($"调用者：{userName} ({userId})");
        UpdateInvokeRecords(userId);
        
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