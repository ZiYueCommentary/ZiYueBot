using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.QQ;

public static class Parser
{
    /// <summary>
    /// 扁平化 QQ 消息，以便于传输给各命令。
    /// </summary>
    public static Message FlattenMessage(JsonNode node, bool ignoreForward = false)
    {
        Message message = new Message();
        string forwardMessage = ""; // 被引用的消息内容
        bool wasMention = false; // 如果上一个消息是提及，则删除一个空格，以便把前后消息看成一个整体。
        foreach (JsonObject segment in node.AsArray())
        {
            switch (segment!["type"]!.GetValue<string>())
            {
                case "reply":
                {
                    if (ignoreForward) continue;
                    message.HasForward = true;
                    try
                    {
                        JsonNode response = SendApiRequest("""
                                                           {
                                                               "action": "get_msg",
                                                               "params": {
                                                                   "message_id": %id%
                                                               }
                                                           }
                                                           """.Replace("%id%",
                                segment["data"]!["id"]!.GetValue<string>()))
                            .GetAwaiter().GetResult();
                        forwardMessage = FlattenMessage(response["data"]!["message"]!, true).Text;
                    }
                    catch (Exception e)
                    {
                        QqEvents.Logger.Warn("获取引用内容出错：", e);
                        forwardMessage = "[未知引用消息]";
                    }

                    wasMention = false;
                    break;
                }
                case "text":
                {
                    message.Text += segment["data"]!["text"]!.GetValue<string>()[(wasMention ? 1 : 0)..].SafeArgument();
                    wasMention = false;
                    break;
                }
                case "image":
                {
                    message.Text += $"\u2402{segment["data"]!["url"]!.GetValue<string>()}\u2403";
                    wasMention = false;
                    break;
                }
                case "at":
                {
                    try
                    {
                        string qq = segment["data"]!["qq"]!.GetValue<string>();
                        if (qq != "all")
                        {
                            message.Text += $"\u2404{qq}\u2405";
                            JsonNode response = SendApiRequest("""
                                                               {
                                                                   "action": "get_stranger_info",
                                                                   "params": {
                                                                       "user_id": %qq%,
                                                                       "no_cache": false
                                                                   }
                                                               }
                                                               """.Replace("%qq%", qq)).GetAwaiter().GetResult();
                            Message.MentionedUinAndName[ulong.Parse(qq)] =
                                response["data"]!["nickname"]!.GetValue<string>();
                        }
                        else
                        {
                            message.Text += "\u24040\u2405";
                            Message.MentionedUinAndName[0] = "全体成员";
                        }
                    }
                    catch (Exception e)
                    {
                        QqEvents.Logger.Warn("用户信息获取失败", e);
                        message.Text += "\u24041\u2405";
                        Message.MentionedUinAndName[1] = "[未知用户]";
                    }

                    wasMention = true;
                    break;
                }
                case "face":
                {
                    message.Text += $"\u2406{segment["data"]!["id"]!.GetValue<string>()}\u2407";
                    wasMention = false;
                    break;
                }
            }
        }

        if (!message.HasForward) return message;

        message.Text = message.Text.Contains(' ')
            ? message.Text.Insert(message.Text.IndexOf(' '), $" \"{forwardMessage}\" ")
            : $"{message.Text} \"{forwardMessage}\"";
        return message;
    }

    public static string HierarchizeMessage(string message)
    {
        bool simpleMessage = true;
        string result = "";
        int pos = 0;
        for (int i = 0; i < message.Length; i++)
        {
            switch (message[i])
            {
                case '\u2402': // 图片
                {
                    result += message.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                    int end = message.IndexOf('\u2403', i + 1);
                    result += $"[CQ:image,url={message.Substring(i + 1, end - i - 1).Replace(",", "&#44;")}]";
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
                case '\u2404': // 提及
                {
                    result += message.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                    int end = message.IndexOf('\u2405', i + 1);
                    string id = message.Substring(i + 1, end - i - 1);
                    if (id == "0")
                    {
                        result += "@全体成员 ";
                    }
                    else
                    {
                        result += $"[CQ:at,qq={id}] "; // 提及后面必须加空格，否则会显示出错。
                    }

                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
                case '\u2406': // 表情
                {
                    result += message.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                    int end = message.IndexOf('\u2407', i + 1);
                    result += $"[CQ:face,id={message.Substring(i + 1, end - i - 1)}]";
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
                case '\u2408': // 本地图片，只在捞云瓶中使用
                {
                    result += message.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                    int end = message.IndexOf('\u2409', i + 1);
                    result +=
                        $"[CQ:image,url=file:///{Path.GetFullPath(message.Substring(i + 1, end - i - 1).Replace(",", "&#44;")).Replace("\\", "/")}]";
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
            }
        }

        if (simpleMessage) return message;

        if (pos < message.Length - 1) result += message[(pos + (message[pos + 1] == ' ' ? 2 : 1))..];
        return result;
    }

    public static async Task SendMessage(EventType eventType, ulong target, string message)
    {
        if (message == "")
        {
            QqEvents.Logger.Warn("尝试发送空内容！");
            return;
        }

        message = message.Replace("&", "&amp;").Replace("[", "&#91;").Replace("]", "&#93;").Replace(@"\", @"\\");
        string request = eventType == EventType.DirectMessage
            ? """
              {
                 "action": "send_private_msg",
                 "params": {
                    "user_id": %target%,
                    "message": "%message%"
                 }
              }
              """
            : """
              {
                 "action": "send_group_msg",
                 "params": {
                    "group_id": %target%,
                    "message": "%message%"
                 }
              }
              """;
        request = request.Replace("%target%", target.ToString());
        request = request.Replace("%message%",
            HierarchizeMessage(message).Replace("\t", "\\t").Replace("\r\n", "\r").Replace("\r", "\\r")
                .Replace("\n", "\\r"));
        await SendApiRequest(request);
    }

    public static async Task<JsonNode> SendApiRequest(string json)
    {
        for (int i = 0; i < 3; i++)
        {
            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
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