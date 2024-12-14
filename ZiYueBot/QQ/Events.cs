using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using log4net;
using ZiYueBot.Harmony;
using ZiYueBot.Core;
using ZiYueBot.General;

namespace ZiYueBot.QQ;

public static class Events
{
    private static readonly ILog Logger = LogManager.GetLogger("QQ 消息解析");

    public static async Task Initialize()
    {
        byte[] buffer = new byte[4096];
        while (ZiYueBot.Instance.QqEvent.State == WebSocketState.Open)
        {
            try
            {
                WebSocketReceiveResult result =
                    await ZiYueBot.Instance.QqEvent.ReceiveAsync(new ArraySegment<byte>(buffer),
                        CancellationToken.None);
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                JsonNode? message = JsonNode.Parse(receivedMessage);
                
                if (message?["message_type"] is null) continue;
                switch (message["message_type"]!.ToString())
                {
                    case "private":
                        await EventHandler(
                            EventType.DirectMessage,
                            message["message"]!,
                            message["user_id"]!.GetValue<uint>(),
                            message["sender"]!["nickname"]!.GetValue<string>(),
                            message["user_id"]!.GetValue<ulong>()
                        );
                        break;
                    case "group":
                        await EventHandler(
                            EventType.GroupMessage,
                            message["message"]!,
                            message["user_id"]!.GetValue<uint>(),
                            message["sender"]!["nickname"]!.GetValue<string>(),
                            message["group_id"]!.GetValue<ulong>()
                        );
                        break;
                }
            }
            catch (NullReferenceException)
            {
            }
            catch (Exception e)
            {
                Logger.Warn(e.Message, e);
            }
        }
    }

    /// <summary>
    /// 处理 QQ 消息。
    /// </summary>
    /// <param name="eventType">消息来源</param>
    /// <param name="message">消息 JSON</param>
    /// <param name="userId">来源用户 ID</param>
    /// <param name="userName">来源用户昵称</param>
    /// <param name="sourceUin">消息所在群 ID 或好友 ID</param>
    private static async Task EventHandler(EventType eventType, JsonNode message, uint userId, string userName,
        ulong sourceUin)
    {
        try
        {
            Message flatten = Parser.FlattenMessage(message);
            if (flatten.Text == "/") return;
            string[] args = Parser.Parse(flatten.Text);
            if (message.AsArray()[0]!["type"]!.GetValue<string>() == "image" && PicFace.Users.Contains(userId))
            {
                string url = message.AsArray()[0]!["data"]!["url"]!.GetValue<string>();
                await Parser.SendMessage(eventType, sourceUin, $"\u2402{url}\u2403\\r{url}");
                PicFace.Users.Remove(userId);
                PicFace.Logger.Info($"{userName} 的表情转图片已完成：{url}");
                return;
            }

            switch (args[0])
            {
                case "win":
                {
                    Win win = Commands.GetGeneralCommand<Win>(Platform.QQ);
                    args[0] = sourceUin.ToString(); // 群聊 ID

                    await Parser.SendMessage(eventType, sourceUin, win.QQInvoke(eventType, userName, userId, args));
                    if (win.SeekWinningCouple(userId, userName, args[0], out string coupleText))
                    {
                        await Parser.SendMessage(eventType, sourceUin, coupleText);
                        await Parser.SendMessage(eventType, sourceUin,
                            $"\u2402file:///{Path.GetFullPath("resources/zvv.jpeg").Replace("\\", "/")}\u2403");
                    }

                    if (win.TryCommonProsperity(userId, userName, args[0], out string prosperityText))
                    {
                        await Parser.SendMessage(eventType, sourceUin, prosperityText);
                    }

                    break;
                }
                case "开始俄罗斯轮盘":
                {
                    StartRevolver startRevolver = Commands.GetHarmonyCommand<StartRevolver>();
                    args[0] = sourceUin.ToString(); // 群聊 ID
                    await Parser.SendMessage(eventType, sourceUin,
                        startRevolver.Invoke(eventType, userName, userId, args));
                    break;
                }
                case "重置俄罗斯轮盘":
                {
                    RestartRevolver startRevolver = Commands.GetHarmonyCommand<RestartRevolver>();
                    args[0] = sourceUin.ToString(); // 群聊 ID
                    await Parser.SendMessage(eventType, sourceUin,
                        startRevolver.Invoke(eventType, userName, userId, args));
                    break;
                }
                case "开枪":
                {
                    Shooting shooting = Commands.GetHarmonyCommand<Shooting>();
                    args[0] = sourceUin.ToString(); // 群聊 ID
                    if (args.Length < 2)
                    {
                        List<string> list = args.ToList();
                        list.Add($"\u2404{userId}\u2405");
                        Message.MentionedUinAndName[userId] = $"@{userName}";
                        args = list.ToArray();
                    }

                    if (args[1].StartsWith('\u2404') && args[1].EndsWith('\u2405'))
                    {
                        args[1] = args[1][1..^1];
                        await Parser.SendMessage(eventType, sourceUin,
                            shooting.Invoke(eventType, userName, userId, args));
                        break;
                    }

                    await Parser.SendMessage(eventType, sourceUin, "参数无效。使用“/help 开枪”查看命令用法。");

                    break;
                }
                case "转轮":
                {
                    Rotating rotating = Commands.GetHarmonyCommand<Rotating>();
                    args[0] = sourceUin.ToString(); // 群聊 ID
                    await Parser.SendMessage(eventType, sourceUin, rotating.Invoke(eventType, userName, userId, args));
                    break;
                }
                case "xibao":
                {
                    Xibao xibao = Commands.GetHarmonyCommand<Xibao>();
                    string result = xibao.Invoke(eventType, userName, userId, args);
                    if (result != "")
                    {
                        await Parser.SendMessage(eventType, sourceUin, result);
                        break;
                    }

                    await Parser.SendMessage(eventType, sourceUin,
                        $"\u2402base64://{Convert.ToBase64String(Xibao.Render(true, args[1]))}\u2403");
                    break;
                }
                case "beibao":
                {
                    Beibao beibao = Commands.GetHarmonyCommand<Beibao>();
                    string result = beibao.Invoke(eventType, userName, userId, args);
                    if (result != "")
                    {
                        await Parser.SendMessage(eventType, sourceUin, result);
                        break;
                    }

                    await Parser.SendMessage(eventType, sourceUin,
                        $"\u2402base64://{Convert.ToBase64String(Xibao.Render(false, args[1]))}\u2403");
                    break;
                }
                case "balogo":
                {
                    BALogo baLogo = Commands.GetHarmonyCommand<BALogo>();
                    string result = baLogo.Invoke(eventType, userName, userId, args);
                    if (result != "")
                    {
                        await Parser.SendMessage(eventType, sourceUin, result);
                        break;
                    }

                    await Parser.SendMessage(eventType, sourceUin,
                        $"\u2402base64://{Convert.ToBase64String(baLogo.Render(args[1], args[2]))}\u2403");
                    break;
                }
                default:
                {
                    if (args[0].Contains("云瓶") && flatten.HasForward)
                    {
                        await Parser.SendMessage(eventType, sourceUin, "使用云瓶命令时不可回复消息！");
                        break;
                    }

                    IHarmonyCommand? harmony = Commands.GetHarmonyCommand<IHarmonyCommand>(args[0]);
                    if (harmony is not null)
                    {
                        await Parser.SendMessage(eventType, sourceUin,
                            harmony.Invoke(eventType, userName, userId, args));
                    }
                    else
                    {
                        IGeneralCommand? general = Commands.GetGeneralCommand<IGeneralCommand>(Platform.QQ, args[0]);
                        if (general is not null)
                        {
                            await Parser.SendMessage(eventType, sourceUin,
                                general.QQInvoke(eventType, userName, userId, args));
                        }
                        else if (flatten.Text.StartsWith('/'))
                        {
                            await Parser.SendMessage(eventType, sourceUin, "未知命令。请使用 /help 查看命令列表。");
                        }
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message, ex);
            await Parser.SendMessage(eventType, sourceUin, "命令解析错误。");
        }
    }
}