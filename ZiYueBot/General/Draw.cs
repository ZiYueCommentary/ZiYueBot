using System.Text;
using System.Text.Json.Nodes;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class Draw : Command
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

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (arg.IsEmpty())
        {
            await context.SendMessage("参数数量不足。使用“/help draw”查看命令用法。");
            return;
        }
        
        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage("频率已达限制（每分钟 1 条）");
            return;
        }

        if (!await ValidateInvoke(context))
        {
            if (DateTime.Today.Month == 5 && DateTime.Today.Day == 3)
            {
                await context.SendMessage("""
                                          今天是子悦的生日，赞助者命令“绘画”对所有人开放。
                                          喜欢的话请考虑在爱发电赞助“子悦机器”方案，以获得赞助者权益。
                                          https://afdian.com/a/ziyuecommentary2020
                                          """);
            }
            else return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);

        try
        {
            JsonNode posted = PostRequest(arg.ToString(context));
            await WebUtils.DownloadFile(
                posted["choices"]![0]!["message"]!["content"]![0]!["image"]!.GetValue<string>(),
                "temp/result.png");
            await context.SendMessage([
                new ImageMessageEntity($"file:///{Path.GetFullPath("temp/result.png").Replace("\\", "/")}", "draw.png")
            ]);
            File.Delete("temp/result.png");
        }
        catch (TimeoutException)
        {
            await context.SendMessage("服务连接超时。");
        }
        catch (HttpRequestException)
        {
            await context.SendMessage("第三方拒绝：涉嫌知识产权风险。");
        }
        catch (Exception e)
        {
            Logger.Error(e.Message, e);
            await context.SendMessage("命令内部错误。");
        }
    }

    private static JsonNode PostRequest(string prompt)
    {
        using HttpClient client = new HttpClient();
        using HttpRequestMessage request =
            new HttpRequestMessage(HttpMethod.Post,
                "https://dashscope.aliyuncs.com/api/v1/services/aigc/multimodal-generation/generation");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", $"Bearer {ZiYueBot.Instance.Config.DeepSeekKey}"); // placeholder
        // 下面这个 json 太复杂了，写成 C# 代码乱得要死，就这样吧。
        using StringContent content = new StringContent("""
                                                        {
                                                            "model": "qwen-image-max",
                                                            "input": {
                                                                "messages": [
                                                                    {
                                                                        "role": "user",
                                                                        "content": [
                                                                            {
                                                                                "text": "%prompt%"
                                                                            }
                                                                        ]
                                                                    }
                                                                ]
                                                            },
                                                            "parameters": {
                                                                "negative_prompt": "",
                                                                "size": "1328*1328",
                                                                "n": 1
                                                            }
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
        return output;
    }

    private static async Task<bool> ValidateInvoke(IContext context)
    {
        await using (MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase())
        {
            await using MySqlCommand command = new MySqlCommand(
                $"SELECT * FROM sponsors WHERE userid = {context.UserId} LIMIT 1",
                connection);
            await using MySqlDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                await context.SendMessage("""
                                          您不是子悦机器的赞助者！
                                          本命令仅供赞助者使用，请在爱发电赞助“子悦机器”方案（￥10.00/年）以调用命令。
                                          https://afdian.com/a/ziyuecommentary2020
                                          """);
                return false;
            }

            if (DateTime.Today > reader.GetDateTime("expiry"))
            {
                await context.SendMessage($"""
                                           您的赞助已过期（{reader.GetDateTime("expiry"):yyyy年MM月dd日}）
                                           子悦机器每次赞助的有效期为 365 天。
                                           本命令仅供赞助者使用，请在爱发电赞助“子悦机器”方案（￥10.00/年）以调用命令。
                                           https://afdian.com/a/ziyuecommentary2020
                                           """);
                return false;
            }
        }

        await using (MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase())
        {
            await using MySqlCommand command =
                new MySqlCommand($"SELECT * FROM draw WHERE userid = {context.UserId} ", connection);
            await using MySqlDataReader reader = command.ExecuteReader();
            if (!reader.Read() || reader.GetDateTime("current_month").ToYearMonth() != DateTime.Today.ToYearMonth())
            {
                await using MySqlConnection updateConnection = ZiYueBot.Instance.ConnectDatabase();
                await using MySqlCommand updateCommand =
                    new MySqlCommand(
                        $"""
                         INSERT INTO draw (userid, current_month, limitation, consumed) VALUES ({context.UserId}, '{DateTime.Today.ToYearMonth():yyyy-M-d}', 50, 0) 
                         ON DUPLICATE KEY UPDATE current_month = '{DateTime.Today.ToYearMonth():yyyy-M-d}', limitation = 50, consumed = 0
                         """,
                        updateConnection);
                updateCommand.ExecuteNonQuery();
            }
        }

        if (DateTime.Today.Month == 5 && DateTime.Today.Day == 3)
        {
            await context.SendMessage("机器绘画中...");
            return true;
        }

        await using (MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase())
        {
            await using MySqlCommand command =
                new MySqlCommand($"SELECT * FROM draw WHERE userid = {context.UserId} ", connection);
            await using MySqlDataReader reader = command.ExecuteReader();
            reader.Read();
            //判断是否无视额度
            bool bypassed = (Privileged.GetPrivilege(context.UserId) & (long)Privilege.BypassDrawLimitation) == 1;
            
            if (reader.GetInt32("consumed") >= reader.GetInt32("limitation") && !bypassed)
            {
                await context.SendMessage("您本月的调用额度已耗尽。");
                return false;
            }

            await using MySqlConnection updateConnection = ZiYueBot.Instance.ConnectDatabase();
            await using MySqlCommand updateCommand =
                new MySqlCommand($"UPDATE draw SET consumed = consumed + 1 WHERE userid = {context.UserId}",
                    updateConnection);
            updateCommand.ExecuteNonQuery();

            await context.SendMessage($"机器绘画中（本月 {reader.GetInt32("consumed") + 1}/{reader.GetInt32("limitation")} 次）");
            return true;
        }
    }

    public override TimeSpan GetRateLimit(IContext context)
    {
        return TimeSpan.FromMinutes(1);
    }
}