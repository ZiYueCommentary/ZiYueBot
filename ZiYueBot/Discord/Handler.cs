using System.Text;
using System.Text.Json.Nodes;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Harmony;
using ZiYueBot.Utils;

namespace ZiYueBot.Discord;

public static class Handler
{
    private static readonly ILog Logger = LogManager.GetLogger("Discord 消息解析");

    private static async void RegisterCommand(SlashCommandBuilder builder)
    {
        try
        {
            try
            {
                await ZiYueBot.Instance.Discord.CreateGlobalApplicationCommandAsync(builder.Build());
            }
            catch (HttpRequestException e)
            {
                Logger.Error("无法连接 Discord 服务器！", e);
            }
            catch (TimeoutException)
            {
                Logger.Warn("连接超时");
            }
        }
        catch (HttpException e)
        {
            if (e.HttpCode == System.Net.HttpStatusCode.MethodNotAllowed)
            {
                Logger.Warn($"命令重复注册：{builder.Name}");
            }
            else
            {
                Logger.Error($"命令注册失败：{builder.Name}", e);
            }
        }
    }

    public static void Initialize()
    {
        ZiYueBot.Instance.Discord.Ready += ClientReady;
        ZiYueBot.Instance.Discord.SlashCommandExecuted += SlashCommandHandler;
    }

    private static void AddCommandsAsChoices(SlashCommandOptionBuilder builder)
    {
        foreach (HarmonyCommand harmony in Commands.HarmonyCommands.Values.ToHashSet())
        {
            builder.AddChoice($"{harmony.Name} ({harmony.Id})", harmony.Id);
        }

        foreach (GeneralCommand general in Commands.GeneralCommands.Values.ToHashSet().Where(general =>
                     general.SupportedPlatform == Platform.Both ||
                     general.SupportedPlatform == Platform.Discord))
        {
            builder.AddChoice($"{general.Name}（{general.Id}）", general.Id);
        }
    }

    private static SlashCommandBuilder EasyCommandBuilder(Command command)
    {
        SlashCommandBuilder builder = new SlashCommandBuilder();
        builder.WithName(command.Id);
        builder.WithDescription(command.Summary);
        return builder;
    }

    private static async Task ClientReady()
    {
        RegisterCommand(EasyCommandBuilder(new Jrrp()));
        RegisterCommand(EasyCommandBuilder(new Hitokoto()));
        RegisterCommand(EasyCommandBuilder(new About()));
        RegisterCommand(EasyCommandBuilder(new Quotations()));
        RegisterCommand(EasyCommandBuilder(new ListDriftbottle()));
        RegisterCommand(EasyCommandBuilder(new PickStraitbottle()));
        RegisterCommand(EasyCommandBuilder(new ListStraitbottle()));
        RegisterCommand(EasyCommandBuilder(new StartRevolver()));
        RegisterCommand(EasyCommandBuilder(new RestartRevolver()));
        RegisterCommand(EasyCommandBuilder(new Rotating()));
        RegisterCommand(EasyCommandBuilder(new Win()));
        RegisterCommand(EasyCommandBuilder(new Stat()));
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new Chat());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("question").WithDescription("问题内容").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new Draw());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("prompt").WithDescription("正向提示词").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new Shooting());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("user").WithDescription("开枪目标").WithRequired(false)
                .WithType(ApplicationCommandOptionType.User);
            builder.AddOption(optionBuilder);
            RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new ThrowStraitbottle());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("content").WithDescription("瓶子内容").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new ThrowDriftbottle());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("content").WithDescription("瓶子内容").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new PickDriftbottle());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("id").WithDescription("瓶子编号").WithRequired(false)
                .WithType(ApplicationCommandOptionType.Integer);
            builder.AddOption(optionBuilder);
            RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new RemoveDriftbottle());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("id").WithDescription("瓶子编号").WithRequired(true)
                .WithType(ApplicationCommandOptionType.Integer);
            builder.AddOption(optionBuilder);
            RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new Help());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("command").WithDescription("获取帮助的命令名").WithRequired(false)
                .WithType(ApplicationCommandOptionType.String);
            AddCommandsAsChoices(optionBuilder);
            builder.AddOption(optionBuilder);
            RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new Ask());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("question").WithDescription("向张维为教授提出问题").WithRequired(false)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new BALogo());
            SlashCommandOptionBuilder optionLeftBuilder = new SlashCommandOptionBuilder();
            optionLeftBuilder.WithName("left").WithDescription("光环左侧文字").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            SlashCommandOptionBuilder optionRightBuilder = new SlashCommandOptionBuilder();
            optionRightBuilder.WithName("right").WithDescription("光环右侧文字").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOptions(optionLeftBuilder, optionRightBuilder);
            RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new Xibao());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("content").WithDescription("喜报内容").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = EasyCommandBuilder(new Beibao());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("content").WithDescription("悲报内容").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            RegisterCommand(builder);
        }
    }

    private static async Task SendComplexMessage(SocketSlashCommand command, string message)
    {
        if (message.Contains('\u2408'))
        {
            string reply = "";
            List<string> images = [];
            int pos = 0;
            for (int i = 0; i < message.Length; i++)
            {
                switch (message[i])
                {
                    case '\u2408':
                    {
                        reply += message.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                        int end = message.IndexOf('\u2409', i + 1);
                        images.Add(message.Substring(i + 1, end - i - 1));
                        i = pos = end;
                        continue;
                    }
                }
            }

            if (pos < message.Length - 1) reply += message[(pos + (message[pos + 1] == ' ' ? 2 : 1))..];

            await command.RespondWithFilesAsync(
                images.ConvertAll(path => new FileAttachment(path, path)),
                reply);
        }
        else
        {
            await command.RespondAsync(message);
        }
    }

    private static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        try
        {
            string userMention = command.User.Mention;
            ulong userId = command.User.Id;
            Message.MentionedUinAndName[userId] = command.User.GlobalName;
            EventType eventType = command.GuildId is null ? EventType.DirectMessage : EventType.GroupMessage;

            if (!Commands.HarmonyCommands.ContainsKey(command.CommandName))
            {
                if (Commands.GetGeneralCommand<GeneralCommand>(Platform.Discord, command.CommandName) is null)
                {
                    await command.RespondAsync("未知命令。请使用 /help 查看命令列表。");
                    return;
                }
            }

            await using (MySqlConnection connection = ZiYueBot.Instance.ConnectDatabase())
            {
                await using MySqlCommand command1 = new MySqlCommand(
                    $"SELECT * FROM blacklists WHERE userid = {userId} AND command = 'all'",
                    connection);
                await using MySqlDataReader reader = command1.ExecuteReader();
                if (reader.Read())
                {
                    await command.RespondAsync($"""
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
                await using MySqlCommand command1 = new MySqlCommand(
                    "SELECT * FROM blacklists WHERE userid = @userid AND command = @command",
                    connection);
                command1.Parameters.AddWithValue("@userid", userId);
                command1.Parameters.AddWithValue("@command", command.CommandName);
                await using MySqlDataReader reader = command1.ExecuteReader();
                if (reader.Read())
                {
                    await command.RespondAsync($"""
                                                您已被禁止使用该命令！
                                                时间：{reader.GetDateTime("time"):yyyy年MM月dd日 HH:mm:ss}
                                                原因：{reader.GetString("reason")}
                                                用户协议：https://docs.ziyuebot.cn/tos.html
                                                """);
                    return;
                }
            }

            switch (command.CommandName)
            {
                case "draw":
                {
                    SocketSlashCommandDataOption? content = command.Data.Options.FirstOrDefault();
                    Draw draw = Commands.GetGeneralCommand<Draw>(Platform.Discord, "draw")!;
                    Draw.InvokeValidation validation = draw.TryInvoke(eventType, userMention, userId,
                        [(string)content!.Value], out string output);
                    if (validation is Draw.InvokeValidation.RateLimited or Draw.InvokeValidation.NotEnoughParameters)
                    {
                        await command.RespondAsync(output);
                        break;
                    }

                    if (validation is Draw.InvokeValidation.SponsorExpired or Draw.InvokeValidation.NotSponsor)
                    {
                        if (DateTime.Today.Month == 5 && DateTime.Today.Day == 3)
                        {
                            await command.Channel.SendMessageAsync("""
                                                                   今天是子悦的生日，赞助者命令“绘画”对所有人开放。
                                                                   喜欢的话请考虑在爱发电赞助“子悦机器”方案，以获得赞助者权益。
                                                                   https://afdian.com/a/ziyuecommentary2020"
                                                                   """);
                        }
                        else
                        {
                            await command.RespondAsync(output);
                            break;
                        }
                    }

                    await command.RespondAsync("机器绘画中...");
                    try
                    {
                        JsonNode drawRequest = draw.PostRequest((string)content.Value);
                        string taskId = drawRequest["task_id"]!.GetValue<string>();
                        ISocketMessageChannel channel = command.Channel;
                        try
                        {
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
                                JsonNode task = JsonNode.Parse(res)!["output"]!;
                                string taskStatus = task["task_status"]!.GetValue<string>();
                                switch (taskStatus)
                                {
                                    case "SUCCEEDED":
                                    {
                                        await WebUtils.DownloadFile(
                                            task["results"]![0]!["url"]!.GetValue<string>(),
                                            "temp/result.png");
                                        await channel.SendFileAsync(new FileAttachment("temp/result.png",
                                            "result.png"));
                                        File.Delete("temp/result.png");
                                        return;
                                    }
                                    case "FAILED":
                                    {
                                        await channel.SendMessageAsync(
                                            $"任务执行失败：{task["message"]!.GetValue<string>()}");
                                        return;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e.Message, e);
                            await command.Channel.SendMessageAsync("命令内部错误。");
                        }
                    }
                    catch (TimeoutException)
                    {
                        await command.Channel.SendMessageAsync("服务连接超时。");
                    }
                    catch (HttpRequestException)
                    {
                        await command.Channel.SendMessageAsync("第三方拒绝：涉嫌知识产权风险。");
                    }
                    catch (Exception)
                    {
                        await command.Channel.SendMessageAsync("命令内部错误。");
                    }

                    break;
                }
                case "win":
                {
                    Win win = Commands.GetGeneralCommand<Win>(Platform.Discord, "win")!;
                    string guildId = eventType == EventType.GroupMessage ? ((ulong)command.GuildId!).ToString() : "-1";
                    await command.RespondAsync(win.DiscordInvoke(
                        eventType,
                        userMention, userId,
                        [guildId]));
                    if (win.SeekWinningCouple(userId, userMention, guildId, out string coupleText))
                    {
                        await command.Channel.SendFileAsync(
                            new FileAttachment("resources/zvv.jpeg", "zvv.jpeg"),
                            coupleText);
                    }

                    if (win.TryCommonProsperity(userId, userMention, guildId, out string prosperityText))
                    {
                        await command.Channel.SendMessageAsync(prosperityText);
                    }

                    break;
                }
                case "开始俄罗斯轮盘":
                {
                    await command.RespondAsync(Commands.GetHarmonyCommand<StartRevolver>("开始俄罗斯轮盘")!.Invoke(
                        eventType,
                        userMention, userId,
                        [((ulong)command.ChannelId!).ToString()]));
                    break;
                }
                case "重置俄罗斯轮盘":
                {
                    await command.RespondAsync(Commands.GetHarmonyCommand<RestartRevolver>("重置俄罗斯轮盘")!.Invoke(
                        eventType,
                        userMention, userId,
                        [((ulong)command.ChannelId!).ToString()]));
                    break;
                }
                case "开枪":
                {
                    SocketSlashCommandDataOption? user = command.Data.Options.FirstOrDefault();
                    if (user is not null && user.Value is not SocketGuildUser) user = null;
                    ulong target = ((SocketGuildUser)user!.Value)?.Id ?? command.User.Id;
                    Message.MentionedUinAndName[target] = $"<@{target}>";
                    await command.RespondAsync(Commands.GetHarmonyCommand<Shooting>("开枪")!.Invoke(
                        eventType,
                        userMention, userId,
                        [((ulong)command.ChannelId!).ToString(), target.ToString()]));
                    break;
                }
                case "转轮":
                {
                    await command.RespondAsync(Commands.GetHarmonyCommand<Rotating>("转轮")!.Invoke(
                        eventType,
                        userMention, userId,
                        [((ulong)command.ChannelId!).ToString()]));
                    break;
                }
                case "ask":
                {
                    SocketSlashCommandDataOption? question = command.Data.Options.FirstOrDefault();
                    await command.RespondAsync(Commands.GetHarmonyCommand<Ask>("ask")!.Invoke(eventType,
                        userMention, userId,
                        question is null ? ["ask"] : ["", (string)question.Value]));
                    break;
                }
                case "help":
                {
                    SocketSlashCommandDataOption? first = command.Data.Options.FirstOrDefault();
                    await command.RespondAsync(Commands.GetGeneralCommand<Help>(Platform.Discord, "help")!
                        .DiscordInvoke(eventType, userMention, userId,
                            ["help", first is null ? "" : (string)first.Value]));
                    break;
                }
                case "balogo":
                {
                    SocketSlashCommandDataOption? left = command.Data.Options.ToList()[0];
                    SocketSlashCommandDataOption? right = command.Data.Options.ToList()[1];
                    BALogo baLogo = Commands.GetHarmonyCommand<BALogo>("balogo")!;
                    string result = baLogo.Invoke(eventType, userMention, userId,
                        ["balogo", (string)left.Value, (string)right.Value]);
                    if (result == "")
                    {
                        await command.RespondWithFileAsync(new FileAttachment(
                            new MemoryStream(baLogo.Render((string)left.Value, (string)right.Value)), "balogo.png"));
                        break;
                    }

                    await command.RespondAsync(result);
                    break;
                }
                case "xibao":
                {
                    SocketSlashCommandDataOption? content = command.Data.Options.FirstOrDefault();
                    Xibao xibao = Commands.GetHarmonyCommand<Xibao>("xibao")!;
                    string result = xibao.Invoke(eventType, userMention, userId,
                        ["xibao", (string)content!.Value]);
                    if (result == "")
                    {
                        await command.RespondWithFileAsync(new FileAttachment(
                            new MemoryStream(Xibao.Render(true, (string)content.Value)), "xibao.png"));
                        break;
                    }

                    await command.RespondAsync(result);
                    break;
                }
                case "beibao":
                {
                    SocketSlashCommandDataOption? content = command.Data.Options.FirstOrDefault();
                    Beibao beibao = Commands.GetHarmonyCommand<Beibao>("beibao")!;
                    string result = beibao.Invoke(eventType, userMention, userId,
                        ["beibao", (string)content!.Value]);
                    if (result == "")
                    {
                        await command.RespondWithFileAsync(new FileAttachment(
                            new MemoryStream(Xibao.Render(false, (string)content.Value)), "beibao.png"));
                        break;
                    }

                    await command.RespondAsync(result);
                    break;
                }
                case "chat":
                {
                    SocketSlashCommandDataOption? content = command.Data.Options.FirstOrDefault();
                    Chat chat = Commands.GetGeneralCommand<Chat>(Platform.Discord, "chat")!;
                    string result = chat.DiscordInvoke(eventType, userMention, userId, [(string)content!.Value]);
                    if (result != "")
                    {
                        await command.RespondAsync(result);
                    }
                    else
                    {
                        await command.RespondAsync("深度思考中...");
                        try
                        {
                            DateTime prev = DateTime.Now;
                            JsonNode node = chat.PostQuestion(false, (string)content.Value)["choices"]![0]!["message"]!;
                            DateTime last = DateTime.Now;

                            string think = node["reasoning_content"]!.GetValue<string>();
                            if (think.StartsWith('\n')) think = think[1..];
                            if (think.EndsWith('\n')) think = think[..^1];

                            string[] reason = think.Split('\n');
                            StringBuilder builder = new StringBuilder();
                            builder.Append($"`已深度思考 {Convert.ToInt32(Math.Round((last - prev).TotalSeconds))} 秒`\n\n");
                            foreach (string se in reason)
                            {
                                builder.Append("> ").Append(se).Append('\n');
                            }

                            builder.Append('\n').Append(node["content"]!.GetValue<string>());
                            if (builder.Length > 1900)
                            {
                                builder.Remove(1900, builder.Length - 1900);
                                builder.Append("\n**内容超过 Discord 消息限制，以下内容已被截断。**");
                            }

                            await command.Channel.SendMessageAsync(builder.ToString());
                        }
                        catch (TimeoutException)
                        {
                            await command.Channel.SendMessageAsync("DeepSeek 服务连接超时。");
                        }
                        catch (TaskCanceledException)
                        {
                            await command.Channel.SendMessageAsync("DeepSeek 回答超时。");
                        }
                        catch (Exception)
                        {
                            await command.Channel.SendMessageAsync("命令内部错误。");
                        }
                    }

                    break;
                }
                case "扔海峡云瓶":
                {
                    SocketSlashCommandDataOption? content = command.Data.Options.FirstOrDefault();
                    ThrowStraitbottle throwStraitbottle =
                        Commands.GetGeneralCommand<ThrowStraitbottle>(Platform.Discord, "扔海峡云瓶")!;
                    await command.RespondAsync(throwStraitbottle.DiscordInvoke(eventType, userMention,
                        userId,
                        ["扔海峡云瓶", (string)content!.Value]));
                    break;
                }
                case "捞海峡云瓶":
                {
                    PickStraitbottle pickStraitbottle =
                        Commands.GetGeneralCommand<PickStraitbottle>(Platform.Discord, "捞海峡云瓶")!;
                    string result =
                        pickStraitbottle.DiscordInvoke(eventType, userMention, userId, ["捞海峡云瓶"]);
                    await SendComplexMessage(command, result);
                    break;
                }
                case "删除云瓶":
                {
                    SocketSlashCommandDataOption? content = command.Data.Options.FirstOrDefault();
                    RemoveDriftbottle removeDriftbottle =
                        Commands.GetGeneralCommand<RemoveDriftbottle>(Platform.Discord, "删除云瓶")!;
                    await command.RespondAsync(removeDriftbottle.DiscordInvoke(eventType, userMention,
                        userId,
                        [((long)content!.Value).ToString()]));
                    break;
                }
                case "扔云瓶":
                {
                    SocketSlashCommandDataOption? content = command.Data.Options.FirstOrDefault();
                    ThrowDriftbottle throwDriftbottle =
                        Commands.GetGeneralCommand<ThrowDriftbottle>(Platform.Discord, "扔云瓶")!;
                    await command.RespondAsync(throwDriftbottle.DiscordInvoke(eventType, userMention,
                        userId,
                        [(string)content!.Value]));
                    break;
                }
                case "捞云瓶":
                {
                    SocketSlashCommandDataOption? content = command.Data.Options.FirstOrDefault();
                    PickDriftbottle pickDriftbottle =
                        Commands.GetGeneralCommand<PickDriftbottle>(Platform.Discord, "捞云瓶")!;
                    string result = pickDriftbottle.DiscordInvoke(eventType, userMention, userId,
                        [content == null ? int.MinValue.ToString() : ((long)content.Value).ToString()]);
                    await SendComplexMessage(command, result);
                    break;
                }
                default:
                {
                    HarmonyCommand? harmony = Commands.GetHarmonyCommand<HarmonyCommand>(command.CommandName);
                    if (harmony is not null)
                    {
                        await command.RespondAsync(harmony.Invoke(eventType, userMention, userId, []));
                    }
                    else
                    {
                        GeneralCommand? general =
                            Commands.GetGeneralCommand<GeneralCommand>(Platform.Discord, command.CommandName);
                        if (general is not null)
                        {
                            await command.RespondAsync(general.DiscordInvoke(eventType, userMention,
                                userId, []));
                        }
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message, ex);
            await command.RespondAsync("命令解析错误。");
        }
    }
}