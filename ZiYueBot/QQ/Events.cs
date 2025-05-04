using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Harmony;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Utils;

namespace ZiYueBot.QQ;

public static class Events
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

                if (message?["message_type"] is null) continue;
                switch (message["message_type"]!.ToString())
                {
                    case "private":
                        EventHandler(
                            EventType.DirectMessage,
                            message["message"]!,
                            message["user_id"]!.GetValue<uint>(),
                            message["sender"]!["nickname"]!.GetValue<string>(),
                            message["user_id"]!.GetValue<ulong>()
                        );
                        break;
                    case "group":
                        EventHandler(
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

            if (!Commands.HarmonyCommands.ContainsKey(args[0]))
            {
                if (Commands.GetGeneralCommand<IGeneralCommand>(Platform.QQ, args[0]) is null)
                {
                    if (flatten.Text.StartsWith('/'))
                    {
                        await Parser.SendMessage(eventType, sourceUin, "未知命令。请使用 /help 查看命令列表。");
                    }

                    return;
                }
            }

            await using (MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase())
            {
                await using MySqlCommand command = new MySqlCommand(
                    $"SELECT * FROM blacklists WHERE userid = {userId} AND command = 'all'",
                    connection);
                await using MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    await Parser.SendMessage(eventType, sourceUin, $"""
                                                                    您已被禁止使用子悦机器！
                                                                    时间：{reader.GetDateTime("time"):yyyy年MM月dd日 HH:mm:ss}
                                                                    原因：{reader.GetString("reason")}
                                                                    用户协议：https://docs.ziyuebot.cn/tos.html
                                                                    """);
                    return;
                }
            }

            await using (MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase())
            {
                await using MySqlCommand command = new MySqlCommand(
                    "SELECT * FROM blacklists WHERE userid = @userid AND command = @command",
                    connection);
                command.Parameters.AddWithValue("@userid", userId);
                command.Parameters.AddWithValue("@command", args[0]);
                await using MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    await Parser.SendMessage(eventType, sourceUin, $"""
                                                                    您已被禁止使用该命令！
                                                                    时间：{reader.GetDateTime("time"):yyyy年MM月dd日 HH:mm:ss}
                                                                    原因：{reader.GetString("reason")}
                                                                    用户协议：https://docs.ziyuebot.cn/tos.html
                                                                    """);
                    return;
                }
            }

            switch (args[0])
            {
                case "draw":
                {
                    Draw draw = Commands.GetGeneralCommand<Draw>(Platform.QQ, "draw")!;
                    string result = draw.QQInvoke(eventType, userName, userId, args);
                    if (result != "")
                    {
                        await Parser.SendMessage(eventType, sourceUin, result);
                    }
                    else
                    {
                        if (DateTime.Today.Month == 5 && DateTime.Today.Day == 3)
                        {
                            await using MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase();
                            await using MySqlCommand command =
                                new MySqlCommand($"SELECT * FROM sponsors WHERE userid = {userId}", connection);
                            await using MySqlDataReader reader = command.ExecuteReader();
                            if (!reader.Read())
                            {
                                await Parser.SendMessage(eventType, sourceUin, """
                                                                               今天是子悦的生日，赞助者命令“绘画”对所有人开放。
                                                                               喜欢的话请考虑在爱发电赞助“子悦机器”方案，以获得赞助者权益。
                                                                               https://afdian.com/a/ziyuecommentary2020
                                                                               """);
                            }
                        }

                        await Parser.SendMessage(eventType, sourceUin, "机器绘画中...");
                        try
                        {
                            JsonNode posted = draw.PostRequest(string.Join(' ', args[1..]));
                            string taskId = posted["task_id"]!.GetValue<string>();
                            for (;;)
                            {
                                Thread.Sleep(5000);

                                using HttpClient client = new HttpClient();
                                using HttpRequestMessage request =
                                    new HttpRequestMessage(HttpMethod.Get,
                                        $"https://dashscope.aliyuncs.com/api/v1/tasks/{taskId}");
                                request.Headers.Add("Authorization",
                                    $"Bearer {ZiYueBot.Instance.Config.DeepSeekKey}"); // placeholder
                                using HttpResponseMessage response =
                                    client.SendAsync(request).GetAwaiter().GetResult();
                                response.EnsureSuccessStatusCode();
                                string res = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                if (res == "") throw new TimeoutException();
                                JsonNode output = JsonNode.Parse(res)!["output"]!;
                                string taskStatus = output["task_status"]!.GetValue<string>();
                                switch (taskStatus)
                                {
                                    case "SUCCEEDED":
                                    {
                                        await WebUtils.DownloadFile(
                                            output["results"]![0]!["url"]!.GetValue<string>(),
                                            "temp/result.png");
                                        await Parser.SendMessage(eventType, sourceUin,
                                            $"\u2402file:///{Path.GetFullPath("temp/result.png").Replace("\\", "/")}\u2403");
                                        File.Delete("temp/result.png");
                                        return;
                                    }
                                    case "FAILED":
                                    {
                                        await Parser.SendMessage(eventType, sourceUin,
                                            $"任务执行失败：{output["message"]!.GetValue<string>()}");
                                        return;
                                    }
                                }
                            }
                        }
                        catch (TimeoutException)
                        {
                            await Parser.SendMessage(eventType, sourceUin, "服务连接超时。");
                        }
                        catch (HttpRequestException)
                        {
                            await Parser.SendMessage(eventType, sourceUin, "第三方拒绝：涉嫌知识产权风险。");
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e.Message, e);
                            await Parser.SendMessage(eventType, sourceUin, "命令内部错误。");
                        }
                    }

                    break;
                }

                case "chat":
                {
                    Chat chat = Commands.GetGeneralCommand<Chat>(Platform.QQ, "chat")!;
                    string result = chat.QQInvoke(eventType, userName, userId, args);
                    if (result != "")
                    {
                        await Parser.SendMessage(eventType, sourceUin, result);
                    }
                    else
                    {
                        await Parser.SendMessage(eventType, sourceUin, "深度思考中...");
                        try
                        {
                            DateTime prev = DateTime.Now;
                            string answer =
                                chat.PostQuestion(true, string.Join(' ', args[1..]))["choices"]![0]!["message"]![
                                        "content"]!
                                    .GetValue<string>();
                            DateTime last = DateTime.Now;
                            await Parser.SendMessage(eventType, sourceUin,
                                $"已深度思考 {Convert.ToInt32(Math.Round((last - prev).TotalSeconds))} 秒\n\n{answer}");
                        }
                        catch (TimeoutException)
                        {
                            await Parser.SendMessage(eventType, sourceUin, "DeepSeek 服务连接超时。");
                        }
                        catch (TaskCanceledException)
                        {
                            await Parser.SendMessage(eventType, sourceUin, "DeepSeek 回答超时。");
                        }
                    }

                    break;
                }
                case "win":
                {
                    Win win = Commands.GetGeneralCommand<Win>(Platform.QQ, "win")!;
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
                    StartRevolver startRevolver = Commands.GetHarmonyCommand<StartRevolver>("开始俄罗斯轮盘")!;
                    args[0] = sourceUin.ToString(); // 群聊 ID
                    await Parser.SendMessage(eventType, sourceUin,
                        startRevolver.Invoke(eventType, userName, userId, args));
                    break;
                }
                case "重置俄罗斯轮盘":
                {
                    RestartRevolver startRevolver = Commands.GetHarmonyCommand<RestartRevolver>("重置俄罗斯轮盘")!;
                    args[0] = sourceUin.ToString(); // 群聊 ID
                    await Parser.SendMessage(eventType, sourceUin,
                        startRevolver.Invoke(eventType, userName, userId, args));
                    break;
                }
                case "开枪":
                {
                    Shooting shooting = Commands.GetHarmonyCommand<Shooting>("开枪");
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
                    Rotating rotating = Commands.GetHarmonyCommand<Rotating>("转轮")!;
                    args[0] = sourceUin.ToString(); // 群聊 ID
                    await Parser.SendMessage(eventType, sourceUin,
                        rotating.Invoke(eventType, userName, userId, args));
                    break;
                }
                case "xibao":
                {
                    Xibao xibao = Commands.GetHarmonyCommand<Xibao>("xibao")!;
                    string result = xibao.Invoke(eventType, userName, userId, args);
                    if (result != "")
                    {
                        await Parser.SendMessage(eventType, sourceUin, result);
                        break;
                    }

                    await Parser.SendMessage(eventType, sourceUin,
                        $"\u2402base64://{Convert.ToBase64String(Xibao.Render(true, string.Join(' ', args[1..])))}\u2403");
                    break;
                }
                case "beibao":
                {
                    Beibao beibao = Commands.GetHarmonyCommand<Beibao>("beibao")!;
                    string result = beibao.Invoke(eventType, userName, userId, args);
                    if (result != "")
                    {
                        await Parser.SendMessage(eventType, sourceUin, result);
                        break;
                    }

                    await Parser.SendMessage(eventType, sourceUin,
                        $"\u2402base64://{Convert.ToBase64String(Xibao.Render(false, string.Join(' ', args[1..])))}\u2403");
                    break;
                }
                case "balogo":
                {
                    BALogo baLogo = Commands.GetHarmonyCommand<BALogo>("balogo")!;
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
                        IGeneralCommand? general =
                            Commands.GetGeneralCommand<IGeneralCommand>(Platform.QQ, args[0]);
                        if (general is not null)
                        {
                            await Parser.SendMessage(eventType, sourceUin,
                                general.QQInvoke(eventType, userName, userId, args));
                        }
                    }

                    break;
                }
            }
        }
        catch
            (HttpRequestException)
        {
            await Parser.SendMessage(eventType, sourceUin, "与服务器通讯失败。");
            await ZiYueBot.Instance.QqEvent.CloseAsync(WebSocketCloseStatus.InternalServerError, String.Empty,
                CancellationToken.None);
            await ZiYueBot.Instance.QqEvent.ConnectAsync(new Uri("ws://127.0.0.1:3001/event/"),
                CancellationToken.None);
            await ZiYueBot.Instance.QqApi.CloseAsync(WebSocketCloseStatus.InternalServerError, String.Empty,
                CancellationToken.None);
            await ZiYueBot.Instance.QqApi.ConnectAsync(new Uri("ws://127.0.0.1:3001/api/"), CancellationToken.None);
            Logger.Info("已重新建立与 QQ 的连接");
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message, ex);
            await Parser.SendMessage(eventType, sourceUin, "命令解析错误。");
        }
    }
}