using System.Text;
using System.Text.Json.Nodes;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public class Chat : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("对话");
    private static readonly string SystemPrompt;

    static Chat()
    {
        using FileStream stream = new FileStream("resources/system.md", FileMode.OpenOrCreate);
        using StreamReader reader = new StreamReader(stream);
        SystemPrompt = reader.ReadToEnd().Replace("\r", "\\r").Replace("\n", "\\n");
    }

    public override string Id => "chat";

    public override string Name => "对话";

    public override string Summary => "与通义千问对话";

    public override string Description => """
                                          /chat [question]
                                          与通义千问对话。“question”是询问的问题。本命令的回复需要很长时间。
                                          每次提问都算作一次新对话。
                                          频率限制：QQ 每次调用间隔 5 分钟；Discord 每次调用间隔 1 分钟；赞助者均为 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/chat
                                          """;

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (arg.IsEmpty())
        {
            await context.SendMessage("参数数量不足。使用“/help chat”查看命令用法。");
            return;
        }

        if (!this.TryPassRateLimit(context))
        {
            if (context.Platform == Platform.Discord) await context.SendMessage("频率已达限制（每分钟 1 条）");
            else await context.SendMessage("频率已达限制（5 分钟 1 条；赞助者每分钟 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);

        await context.SendMessage("机器思考中...");

        try
        {
            DateTime prev = DateTime.Now;
            using HttpClient client = new HttpClient();
            using HttpRequestMessage request =
                new HttpRequestMessage(HttpMethod.Post,
                    "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {ZiYueBot.Instance.Config.DeepSeekKey}");
            JsonObject jsonContent = new JsonObject
            {
                ["messages"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["content"] = SystemPrompt + (DateTime.Today.Month == 4 && DateTime.Today.Day == 1
                            ? "用贴吧风格回答之后的问题，要尽量刻薄。"
                            : ""),
                        ["role"] = "system"
                    },
                    new JsonObject
                    {
                        ["content"] = $"我叫 “{context.UserName}”，一名 {context.Platform} 用户。",
                        ["role"] = "user"
                    },
                    new JsonObject
                    {
                        ["content"] = arg.ToString(context),
                        ["role"] = "user"
                    }
                },
                ["enable_search"] = true,
                ["model"] = "qwen3-max-2026-01-23"
            };
            using StringContent content = new StringContent(jsonContent.ToJsonString(), Encoding.UTF8, "application/json");
            request.Content = content;
            using HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            JsonNode? result = await JsonNode.ParseAsync(await response.Content.ReadAsStreamAsync());

            DateTime last = DateTime.Now;
            StringBuilder builder = new StringBuilder();
            builder.Append($"`已思考 {Convert.ToInt32(Math.Round((last - prev).TotalSeconds))} 秒`\n\n");
            builder.Append(result!["content"]!.GetValue<string>());
            if (builder.Length > 1900)
            {
                builder.Remove(1900, builder.Length - 1900);
                builder.Append("\n**内容过长，以下内容已被截断。**");
            }

            await context.SendMessage(builder.ToString());
        }
        catch (TimeoutException)
        {
            await context.SendMessage("服务连接超时。");
        }
        catch (TaskCanceledException)
        {
            await context.SendMessage("回答超时。");
        }
        catch (Exception)
        {
            await context.SendMessage("命令内部错误。");
        }
    }

    public override TimeSpan GetRateLimit(IContext context)
    {
        if (context.Platform == Platform.Discord) return TimeSpan.FromMinutes(1);

        using MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command = new MySqlCommand(
            $"SELECT * FROM sponsors WHERE userid = {context.UserId} LIMIT 1",
            connection);
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read() && DateTime.Today <= reader.GetDateTime("expiry"))
        {
            return TimeSpan.FromMinutes(1);
        }

        return TimeSpan.FromMinutes(5);
    }
}