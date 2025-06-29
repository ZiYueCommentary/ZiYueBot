using System.Text;
using System.Text.Json.Nodes;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class Chat : GeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("对话");
    private static readonly string SystemPrompt;

    static Chat()
    {
        using FileStream stream = new FileStream("resources/system.md", FileMode.OpenOrCreate);
        using StreamReader reader = new StreamReader(stream);
        SystemPrompt = reader.ReadToEnd().JsonFriendly();
    }

    public override string Id => "chat";

    public override string Name => "对话";

    public override string Summary => "与 DeepSeek R1 对话";

    public override string Description => """
                                          /chat [question] （测试中命令）
                                          与 DeepSeek R1 对话。“question”是询问的问题。本命令的回复需要很长时间。
                                          在 QQ 的回答相比于 Discord 会更短，并且不包括思考过程。
                                          每次提问都算作一次新对话。不支持联网回答。
                                          频率限制（暂时）：QQ 每次调用间隔 5 分钟；Discord 每次调用间隔 1 分钟；赞助者均为 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/general/chat
                                          """;

    public override Platform SupportedPlatform => Platform.Both;

    public JsonNode PostQuestion(bool qq, string question)
    {
        using HttpClient client = new HttpClient();
        using HttpRequestMessage request =
            new HttpRequestMessage(HttpMethod.Post,
                "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", $"Bearer {ZiYueBot.Instance.Config.DeepSeekKey}");
        using StringContent content = new StringContent("""
                {
                  "messages": [
                    {
                      "content": "%system_prompt% %april%",
                      "role": "system"
                    },
                    {
                      "content": "%question%",
                      "role": "user"
                    }
                  ],
                  "model": "deepseek-r1"
                }
                """.Replace("%system_prompt%", SystemPrompt)
                .Replace("%april%", DateTime.Today.Month == 4 && DateTime.Today.Day == 1 ? "用贴吧风格回答之后的问题，要尽量刻薄。" : "")
                .Replace("%question%", question.JsonFriendly())
            /*.Replace("%token%", (qq ? 1024 : 4096).ToString())*/, Encoding.UTF8, "application/json");
        request.Content = content;
        using HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
        Logger.Warn(response.Content);
        response.EnsureSuccessStatusCode();
        string res = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (res == "") throw new TimeoutException();
        return JsonNode.Parse(res)!;
    }

    public override IEnumerable<string> Invoke(Platform platform, EventType eventType, string userName, ulong userId,
        string[] args)
    {
        if (args.Length < 2)
        {
            yield return "参数数量不足。使用“/help chat”查看命令用法。";
            yield break;
        }

        if (!RateLimit.TryPassRateLimit(this, platform, eventType, userId))
        {
            if (platform == Platform.QQ)
            {
                yield return "频率已达限制（5 分钟 1 条；赞助者每分钟 1 条）";
            }
            else
            {
                yield return "频率已达限制（1 分钟 1 条）";
            }

            yield break;
        }

        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        UpdateInvokeRecords(userId);

        yield return "深度思考中...";
        DateTime prev = DateTime.Now;

        using HttpClient client = new HttpClient();
        using HttpRequestMessage request =
            new HttpRequestMessage(HttpMethod.Post,
                "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", $"Bearer {ZiYueBot.Instance.Config.DeepSeekKey}");
        using StringContent requestContent = new StringContent("""
                {
                  "messages": [
                    {
                      "content": "%system_prompt% %april%",
                      "role": "system"
                    },
                    {
                      "content": "%question%",
                      "role": "user"
                    }
                  ],
                  "model": "deepseek-r1"
                }
                """.Replace("%system_prompt%", SystemPrompt)
                .Replace("%april%", DateTime.Today.Month == 4 && DateTime.Today.Day == 1 ? "用贴吧风格回答之后的问题，要尽量刻薄。" : "")
                .Replace("%question%", string.Join(' ', args[1..]).JsonFriendly())
            /*.Replace("%token%", (qq ? 1024 : 4096).ToString())*/, Encoding.UTF8, "application/json");
        request.Content = requestContent;
        using HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
        {
            yield return "DeepSeek 服务连接错误。";
            yield break;
        }

        string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (responseContent == "")
        {
            yield return "DeepSeek 服务连接超时。";
            yield break;
        }

        DateTime last = DateTime.Now;
        JsonNode responseResult = JsonNode.Parse(responseContent)!;
        string reasoningContent = responseResult["reasoning_content"]!.GetValue<string>();
        if (reasoningContent.StartsWith('\n')) reasoningContent = reasoningContent[1..];
        if (reasoningContent.EndsWith('\n')) reasoningContent = reasoningContent[..^1];

        string[] reasons = reasoningContent.Split('\n');
        string content = responseResult["content"]!.GetValue<string>();
        StringBuilder builder = new StringBuilder();
        builder.Append($"`已深度思考 {(last - prev).Seconds} 秒`\n\n");
        if (platform == Platform.Discord)
        {
            foreach (string reason in reasons)
            {
                builder.Append("> ").Append(reason);
            }

            if (builder.Length + content.Length > 1900) // 如果消息过长...
            {
                yield return builder.ToString(); // 拆成两条消息
                if (content.Length > 1900) // 如果还是过长...
                {
                    yield return content[..1900] + "\n**内容超过 Discord 消息限制，以下内容已被截断。**"; // 不管了直接砍！
                }
                else
                {
                    yield return content;
                }

                yield break;
            }

            builder.Append('\n').Append(content);
        }
        else
        {
            builder.Append(content);
        }

        yield return builder.ToString();
    }

    public override TimeSpan GetRateLimit(Platform platform, EventType eventType, ulong userId)
    {
        using MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command = new MySqlCommand(
            $"SELECT * FROM sponsors WHERE userid = {userId} LIMIT 1",
            connection);
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read() && DateTime.Today <= reader.GetDateTime("expiry"))
        {
            return TimeSpan.FromMinutes(1);
        }

        return platform == Platform.Discord ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(5);
    }
}