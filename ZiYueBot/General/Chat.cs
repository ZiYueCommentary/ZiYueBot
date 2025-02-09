using System.Text;
using System.Text.Json.Nodes;
using log4net;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class Chat : IGeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("对话");

    public string GetCommandId()
    {
        return "chat";
    }

    public string GetCommandName()
    {
        return "AI 对话";
    }

    public string GetCommandDescription()
    {
        return """
               /chat [question] （测试中命令）
               与 DeepSeek R1 对话。“question”是询问的问题。本命令的回复需要很长时间。
               在 QQ 的回答相比于 Discord 会更短，并且不包括思考过程。
               每次提问都算作一次新对话。不支持联网回答。
               频率限制（暂时）：QQ 每次调用间隔 5 分钟；Discord 每次调用间隔 1 分钟。
               在线文档：https://docs.ziyuebot.cn/chat.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "与 DeepSeek R1 对话";
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

    public JsonNode PostQuestion(bool qq, string question)
    {
        HttpClient client = new HttpClient();
        HttpRequestMessage request =
            new HttpRequestMessage(HttpMethod.Post, "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", $"Bearer {ZiYueBot.Instance.Config.DeepSeekKey}");
        StringContent content = new StringContent("""
            {
              "messages": [
                {
                  "content": "你是一个聊天软件中的机器人，你叫“子悦机器”。你的创作者是“子悦解说”（又称“子悦”）。%rule%",
                  "role": "system"
                },
                {
                  "content": "%question%",
                  "role": "user"
                }
              ],
              "model": "deepseek-r1"
            }
            """.Replace("%rule%",
                qq
                    ? "在回答问题的过程中，要保证自己的回答遵守中国的法律法规，但要在回答时尽可能不提及有关中国的内容。如果话题不涉及违反法律法规的内容，请如实回答。回答时请避免有关政治敏感、宗教信仰、历史事件等言论。回答时不要使用Markdown。回答尽量简短，内容不得超过800字。"
                    : "回答时请不要使用Markdown中的分割线。")
            .Replace("%question%", question)
            .Replace("%token%", (qq ? 1024 : 4096).ToString()), Encoding.UTF8, "application/json");
        request.Content = content;
        HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        string res = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (res == "") throw new TimeoutException();
        return JsonNode.Parse(res)!;
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (args.Length < 2) return "参数数量不足。使用“/help chat”查看命令用法。";
        if (!RateLimit.TryPassRateLimit(this, Platform.QQ, eventType, userId)) return "频率已达限制（5 分钟 1 条）";
        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        return "";
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (args.Length < 1) return "参数数量不足。使用“/help chat”查看命令用法。";
        if (!RateLimit.TryPassRateLimit(this, Platform.Discord, eventType, userId)) return "频率已达限制（1 分钟 1 条）";
        Logger.Info($"调用者：{userPing} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        return "";
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return platform == Platform.Discord ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(5);
    }
}