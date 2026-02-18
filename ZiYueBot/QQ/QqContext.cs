using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using ZiYueBot.Core;

namespace ZiYueBot.QQ;

public class QqContext(EventType eventType, string userName, ulong userId, uint sourceUin) : IContext
{
    public override Platform Platform => Platform.QQ;
    public override EventType EventType { get; } = eventType;
    public override string UserName { get; } = userName;
    public override ulong UserId { get; } = userId;

    /// <summary>
    /// 消息来源。根据 EventType 而变化，可能为群聊 ID，或用户 ID。
    /// </summary>
    public uint SourceUni { get; } = sourceUin;

    public override async Task SendMessage(MessageChain messageChain)
    {
        IEnumerable<JsonObject> array = messageChain.Select(message =>
        {
            return message.Type switch
            {
                MessageEntityType.Text => new JsonObject
                {
                    ["type"] = "text", ["data"] = new JsonObject { ["text"] = ((TextMessageEntity)message).Text }
                },
                MessageEntityType.Image => new JsonObject
                {
                    ["type"] = "image", ["data"] = new JsonObject { ["file"] = ((ImageMessageEntity)message).Path }
                },
                MessageEntityType.Ping => new JsonObject
                {
                    ["type"] = "at", ["data"] = new JsonObject { ["qq"] = ((PingMessageEntity)message).UserId }
                },
                _ => throw new InvalidDataException("未知消息实体类型")
            };
        });
        await SendApiRequest(new JsonObject
        {
            ["action"] = "send_group_msg",
            ["params"] = new JsonObject
            {
                [EventType == EventType.DirectMessage ? "user_id" : "group_id"] = SourceUni,
                ["message"] = new JsonArray(array.ToArray<JsonNode?>())
            }
        });
    }

    public override async Task<string> FetchUserName(ulong userId)
    {
        if (userId == 0) return "全体成员";
        try
        {
            JsonNode response = await SendApiRequest(
                new JsonObject
                {
                    ["action"] = "get_stranger_info",
                    ["params"] = new JsonObject
                    {
                        ["user_id"] = userId,
                        ["no_cache"] = false
                    }
                });
            return response["data"]!["nickname"]!.GetValue<string>();
        }
        catch (Exception e)
        {
            QqEvents.Logger.Warn($"用户信息获取失败：{userId}", e);
            return "[未知用户]";
        }
    }

    public MessageChain FetchMessageContent(string messageId, out ulong authorUserId)
    {
        MessageChain message;
        try
        {
            JsonNode response = SendApiRequest(new JsonObject
            {
                ["action"] = "get_msg",
                ["params"] = new JsonObject
                {
                    ["message_id"] = messageId
                }
            }).GetAwaiter().GetResult();
            message = Parser.ParseMessage(response["data"]!["message"]!, out _);
            authorUserId = response["data"]!["user_id"]!.GetValue<ulong>();
        }
        catch (Exception e)
        {
            QqEvents.Logger.Warn("获取引用内容出错：", e);
            message = [new TextMessageEntity("[未知引用消息]")];
            authorUserId = 0;
        }

        return message;
    }

    private static async Task<JsonNode> SendApiRequest(JsonObject json)
    {
        for (int i = 0; i < 3; i++)
        {
            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(
                json.ToJsonString(new JsonSerializerOptions
                    { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping })));
            await ZiYueBot.Instance.QqApi.SendAsync(bytesToSend, WebSocketMessageType.Text, true,
                CancellationToken.None);
            byte[] buffer = new byte[4096];
            StringBuilder builder = new StringBuilder();
            WebSocketReceiveResult result;
            do
            {
                result = await ZiYueBot.Instance.QqApi.ReceiveAsync(new ArraySegment<byte>(buffer),
                    CancellationToken.None);
                string chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                builder.Append(chunk);
            } while (!result.EndOfMessage);

            JsonNode? response = JsonNode.Parse(builder.ToString());
            if (response is not null) return response;
        }

        QqEvents.Logger.Error($"API 请求失败：{json}");
        throw new HttpRequestException();
    }
}