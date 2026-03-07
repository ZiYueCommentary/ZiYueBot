using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using log4net;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Utils;

namespace ZiYueBot.QQ;

public static class QqEvents
{
    internal static readonly ILog Logger = LogManager.GetLogger("QQ 消息解析");

    public static async Task Initialize()
    {
        while (ZiYueBot.Instance.QqEvent.State == WebSocketState.Open)
        {
            try
            {
                byte[] buffer = new byte[4096];
                StringBuilder builder = new StringBuilder();
                WebSocketReceiveResult result;
                do
                {
                    result = await ZiYueBot.Instance.QqEvent.ReceiveAsync(new ArraySegment<byte>(buffer),
                        CancellationToken.None);
                    string chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    builder.Append(chunk);
                } while (!result.EndOfMessage);

                JsonNode? message = JsonNode.Parse(builder.ToString());
                uint userId = message!["user_id"]!.GetValue<uint>();

                // 检查云瓶星标
                if (message["notice_type"]?.ToString() == "group_msg_emoji_like")
                {
                    string emoji = message["likes"]![0]!["emoji_id"]!.ToString();
                    if (emoji is "128077" or "76" && message["is_add"]!.GetValue<bool>())
                    {
                        JsonNode response = await QqContext.SendApiRequest(new JsonObject
                        {
                            ["action"] = "get_msg",
                            ["params"] = new JsonObject
                            {
                                ["message_id"] = message["message_id"]!.GetValue<long>().ToString()
                            }
                        });
                        MessageChain chain = Parser.ParseMessage(response["data"]!["message"]!, out _);
                        ulong authorUserId = response["data"]!["user_id"]!.GetValue<ulong>();
                        QqContext context = new QqContext(EventType.GroupMessage, "", userId,
                            response["data"]!["group_id"]!.GetValue<uint>());
                        if (authorUserId != 3793013714) continue;
                        Match match = Stargazers.StargazerRegex().Match(chain.ToString().FirstLine());
                        if (match.Success)
                        {
                            string stargazer = Stargazers.AddStargazer(userId,
                                await context.FetchUserName(userId),
                                int.Parse(match.Groups[1].Value), true);
                            if (!string.IsNullOrEmpty(stargazer)) await context.SendMessage(stargazer);
                        }
                    }

                    continue;
                }

                // 一般消息
                if (message["message_type"] is null) continue;

                switch (message["message_type"]!.ToString())
                {
                    case "private":
                    {
                        QqContext context = new QqContext(EventType.DirectMessage,
                            message["sender"]!["nickname"]!.GetValue<string>(), userId,
                            message["user_id"]!.GetValue<uint>());
                        _ = EventHandler(context, message["message"]!);
                        break;
                    }
                    case "group":
                    {
                        QqContext context = new QqContext(EventType.GroupMessage,
                            message["sender"]!["nickname"]!.GetValue<string>(), userId,
                            message["group_id"]!.GetValue<uint>());
                        _ = EventHandler(context, message["message"]!);
                        break;
                    }
                }
            }
            catch (NullReferenceException)
            {
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
            }
        }
    }

    /// <summary>
    /// 处理 QQ 消息。
    /// </summary>
    private static async Task EventHandler(QqContext context, JsonNode node)
    {
        try
        {
            MessageChain chain = Parser.ParseMessage(node, out MessageChain? forwardMessage, context);
            if (chain.IsEmpty() || chain.ToString(context) == "/") return;
            if (node.AsArray()[0]!["type"]!.GetValue<string>() == "image" && PicFace.Users.Contains(context.UserId))
            {
                string url = node.AsArray()[0]!["data"]!["url"]!.GetValue<string>();
                await context.SendMessage([new ImageMessageEntity(url, ""), new TextMessageEntity(url)]);
                PicFace.Users.Remove(context.UserId);
                PicFace.Logger.Info($"{context.UserName} 的表情转图片已完成：{url}");
                return;
            }

            if (chain[0] is not TextMessageEntity line) return;

            string commandName =
                line.Text.Contains(' ') ? line.Text[..line.Text.IndexOf(' ')].TrimStart('/') : line.Text.TrimStart('/');

            if (Commands.GetCommand(Platform.QQ, commandName) is null)
            {
                if (commandName.StartsWith('/'))
                {
                    await context.SendMessage("未知命令。请使用 /help 查看命令列表。");
                }

                return;
            }

            chain.RemoveAt(0);
            if (line.Text.Contains(' ') && line.Text.IndexOf(' ') != line.Text.Length - 1)
                chain.Insert(0, new TextMessageEntity(line.Text[(line.Text.IndexOf(' ') + 1)..]));
            if (chain.IsEmpty() && forwardMessage is not null) chain = forwardMessage;

            if (await Commands.CheckBlacklist(context, commandName)) return;

            if (commandName.Contains("云瓶") && forwardMessage is not null)
            {
                await context.SendMessage("使用云瓶命令时不可回复消息！");
                return;
            }

            await Commands.GetCommand(Platform.QQ, commandName)!.Invoke(context, chain);
        }
        catch (HttpRequestException)
        {
            await context.SendMessage("与服务器通讯失败。");
            await ZiYueBot.Instance.QqEvent.CloseAsync(WebSocketCloseStatus.InternalServerError, string.Empty,
                CancellationToken.None);
            await ZiYueBot.Instance.QqEvent.ConnectAsync(new Uri("ws://127.0.0.1:3001/event"),
                CancellationToken.None);
            await ZiYueBot.Instance.QqApi.CloseAsync(WebSocketCloseStatus.InternalServerError, string.Empty,
                CancellationToken.None);
            await ZiYueBot.Instance.QqApi.ConnectAsync(new Uri("ws://127.0.0.1:3001/api/"), CancellationToken.None);
            Logger.Info("已重新建立与 QQ 的连接");
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message, ex);
            Logger.Debug(node.ToJsonString());
            await context.SendMessage("命令解析错误。");
        }
    }
}