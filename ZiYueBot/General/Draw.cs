using System.Text;
using System.Text.Json.Nodes;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class Draw : IGeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("绘画");

    public string GetCommandId()
    {
        return "draw";
    }

    public string GetCommandName()
    {
        return "绘画";
    }

    public string GetCommandDescription()
    {
        return """
               /draw [prompt] （赞助者命令）
               通义万相文生图。“prompt”是文生图的提示词。本命令的处理需要很长时间。
               调用此命令后，机器会生成一张1024*1024像素的图片。
               提示词的使用技巧另见：https://help.aliyun.com/zh/model-studio/use-cases/text-to-image-prompt
               该命令仅允许子悦机器的赞助者调用。
               频率限制：每次调用间隔 1 分钟。
               在线文档：https://docs.ziyuebot.cn/general/draw
               """;
    }

    public string GetCommandShortDescription()
    {
        return "通义万相文生图";
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

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
                                                        """.Replace("%prompt%", prompt.Replace("\\", "\\\\")), Encoding.UTF8,
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

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (args.Length < 2) return "参数数量不足。使用“/help draw”查看命令用法。";
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        using (MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase())
        {
            using MySqlCommand command = new MySqlCommand(
                $"SELECT * FROM sponsors WHERE userid = {userId} OR DATE_FORMAT(current_date(), '%m-%d') = '05-03' LIMIT 1",
                connection);
            using MySqlDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return """
                       您不是子悦机器的赞助者！
                       本命令仅供赞助者使用，请在爱发电赞助“子悦机器”方案（￥10.00/年）以调用命令。
                       https://afdian.com/a/ziyuecommentary2020
                       """;
            }

            if (DateTime.Today > reader.GetDateTime("date"))
            {
                return $"""
                       您的赞助已过期（{reader.GetDateTime("date"):yyyy年MM月dd日}）
                       子悦机器每次赞助的有效期为 365 天。
                       本命令仅供赞助者使用，请在爱发电赞助“子悦机器”方案（￥10.00/年）以调用命令。
                       https://afdian.com/a/ziyuecommentary2020
                       """;
            }
        }

        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        return "";
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (args.Length < 1) return "参数数量不足。使用“/help draw”查看命令用法。";
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（每分钟 1 条）";
        using (MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase())
        {
            using MySqlCommand command = new MySqlCommand(
                $"SELECT * FROM sponsors WHERE userid = {userId} OR DATE_FORMAT(current_date(), '%m-%d') = '05-03' LIMIT 1",
                connection);
            using MySqlDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return """
                       您不是子悦机器的赞助者！
                       本命令仅供赞助者使用，请在爱发电赞助“子悦机器”方案（￥10.00/年）以调用命令。
                       https://afdian.com/a/ziyuecommentary2020
                       """;
            }

            if (DateTime.Today > reader.GetDateTime("date"))
            {
                return $"""
                        您的赞助已过期（{reader.GetDateTime("date"):yyyy年MM月dd日}）
                        子悦机器每次赞助的有效期为 365 天。
                        本命令仅供赞助者使用，请在爱发电赞助“子悦机器”方案（￥10.00/年）以调用命令。
                        https://afdian.com/a/ziyuecommentary2020
                        """;
            }
        }

        Logger.Info($"调用者：{userPing} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        return "";
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }
}