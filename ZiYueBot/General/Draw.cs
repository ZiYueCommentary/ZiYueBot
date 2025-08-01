using System.Text;
using System.Text.Json.Nodes;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class Draw : GeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("绘画");

    public override string Id => "draw";

    public override string Name => "绘画";

    public override string Summary => "通义万相文生图";

    public override string Description => """
                                          /draw [prompt] （赞助者命令）
                                          通义万相文生图。“prompt”是文生图的提示词。本命令的处理需要很长时间。
                                          调用此命令后，机器会生成一张1024*1024像素的图片。
                                          提示词的使用技巧另见：https://help.aliyun.com/zh/model-studio/use-cases/text-to-image-prompt
                                          该命令仅允许子悦机器的赞助者调用。
                                          频率限制：每次调用间隔 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/draw
                                          """;

    public override Platform SupportedPlatform => Platform.Both;

    public JsonNode PostRequest(string prompt)
    {
        using HttpClient client = new HttpClient();
        using HttpRequestMessage request =
            new HttpRequestMessage(HttpMethod.Post,
                "https://dashscope.aliyuncs.com/api/v1/services/aigc/text2image/image-synthesis");
        request.Headers.Add("X-DashScope-Async", "enable");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", $"Bearer {ZiYueBot.Instance.Config.DeepSeekKey}"); // placeholder
        using StringContent content = new StringContent("""
                                                        {
                                                        "model": "wanx2.1-t2i-plus",
                                                        "input": {
                                                            "prompt": "%prompt%"
                                                        },
                                                        "parameters": {
                                                            "size": "1024*1024",
                                                            "n": 1
                                                        }
                                                        """.Replace("%prompt%", prompt.JsonFriendly()),
            Encoding.UTF8,
            "application/json");
        request.Content = content;
        using HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        string res = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (res == "") throw new TimeoutException();
        JsonNode output = JsonNode.Parse(res)!["output"]!;
        Logger.Info($"新绘画任务：{output["task_id"]!.GetValue<string>()}");
        return output;
    }

    public override string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        throw new NotSupportedException();
    }

    public override string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        throw new NotSupportedException();
    }

    // 这可能会是未来子悦机器的命令基本框架
    public InvokeValidation TryInvoke(EventType eventType, string userName, ulong userId, string[] args,
        out string output)
    {
        if (args.Length < 1)
        {
            output = "参数数量不足。使用“/help draw”查看命令用法。";
            return InvokeValidation.NotEnoughParameters;
        }

        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId))
        {
            output = "频率已达限制（每分钟 1 条）";
            return InvokeValidation.RateLimited;
        }

        using (MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase())
        {
            using MySqlCommand command = new MySqlCommand(
                $"SELECT * FROM sponsors WHERE userid = {userId} LIMIT 1",
                connection);
            using MySqlDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                output = """
                         您不是子悦机器的赞助者！
                         本命令仅供赞助者使用，请在爱发电赞助“子悦机器”方案（￥10.00/年）以调用命令。
                         https://afdian.com/a/ziyuecommentary2020
                         """;
                return InvokeValidation.NotSponsor;
            }

            if (DateTime.Today > reader.GetDateTime("expiry"))
            {
                output = $"""
                          您的赞助已过期（{reader.GetDateTime("expiry"):yyyy年MM月dd日}）
                          子悦机器每次赞助的有效期为 365 天。
                          本命令仅供赞助者使用，请在爱发电赞助“子悦机器”方案（￥10.00/年）以调用命令。
                          https://afdian.com/a/ziyuecommentary2020
                          """;
                return InvokeValidation.SponsorExpired;
            }
        }

        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        UpdateInvokeRecords(userId);
        output = "";
        return InvokeValidation.Succeed;
    }

    public enum InvokeValidation
    {
        NotEnoughParameters,
        RateLimited,
        NotSponsor,
        SponsorExpired,
        Succeed
    }

    public override TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }
}