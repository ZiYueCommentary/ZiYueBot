using Discord;
using Discord.Net;
using Discord.WebSocket;
using log4net;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Harmony;
using ZiYueBot.QQ;

namespace ZiYueBot.Discord;

public static class Handler
{
    private static readonly ILog Logger = LogManager.GetLogger("Discord 消息解析");

    private static async void RegisterCommand(SlashCommandBuilder builder)
    {
        try
        {
            //todo 换成全局
            try
            {
                await ZiYueBot.Instance.Discord.GetGuild(1152562772941484118)
                    .CreateApplicationCommandAsync(builder.Build());
            }
            catch (HttpRequestException e)
            {
                Logger.Error("无法连接 Discord 服务器！", e);
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
        foreach (IHarmonyCommand harmony in Commands.HarmonyCommands.Values.ToHashSet())
        {
            builder.AddChoice($"{harmony.GetCommandName()} ({harmony.GetCommandId()})", harmony.GetCommandId());
        }

        foreach (IGeneralCommand general in Commands.GeneralCommands.Values.ToHashSet().Where(general =>
                     general.GetSupportedPlatform() == Platform.Both ||
                     general.GetSupportedPlatform() == Platform.Discord))
        {
            builder.AddChoice($"{general.GetCommandName()}（{general.GetCommandId()}）", general.GetCommandId());
        }
    }

    private static SlashCommandBuilder EasyCommandBuilder(ICommand command)
    {
        SlashCommandBuilder builder = new SlashCommandBuilder();
        builder.WithName(command.GetCommandId());
        builder.WithDescription(command.GetCommandShortDescription());
        return builder;
    }

    private static async Task ClientReady()
    {
        RegisterCommand(EasyCommandBuilder(new Jrrp()));
        RegisterCommand(EasyCommandBuilder(new Hitokoto()));
        RegisterCommand(EasyCommandBuilder(new About()));
        RegisterCommand(EasyCommandBuilder(new Quotations()));
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

    private static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        try
        {
            string userMention = command.User.Mention;
            ulong userId = command.User.Id;
            Message.MentionedUinAndName[userId] = command.User.GlobalName;
            switch (command.CommandName)
            {
                case "ask":
                {
                    SocketSlashCommandDataOption? question = command.Data.Options.FirstOrDefault();
                    await command.RespondAsync(Commands.GetHarmonyCommand<Ask>().Invoke(EventType.GroupMessage,
                        userMention, userId,
                        ["ask", question is null ? "" : (string)question.Value]));
                    break;
                }
                case "help":
                {
                    SocketSlashCommandDataOption? first = command.Data.Options.FirstOrDefault();
                    await command.RespondAsync(Commands.GetGeneralCommand<Help>(Platform.Discord)
                        .DiscordInvoke(EventType.GroupMessage, userMention, userId,
                            ["help", first is null ? "" : (string)first.Value]));
                    break;
                }
                case "balogo":
                {
                    SocketSlashCommandDataOption? left = command.Data.Options.ToList()[0];
                    SocketSlashCommandDataOption? right = command.Data.Options.ToList()[1];
                    BALogo baLogo = Commands.GetHarmonyCommand<BALogo>();
                    string result = baLogo.Invoke(EventType.GroupMessage, userMention, userId,
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
                    Xibao xibao = Commands.GetHarmonyCommand<Xibao>();
                    string result = xibao.Invoke(EventType.GroupMessage, userMention, userId,
                        ["xibao", (string)content.Value]);
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
                    Beibao beibao = Commands.GetHarmonyCommand<Beibao>();
                    string result = beibao.Invoke(EventType.GroupMessage, userMention, userId,
                        ["beibao", (string)content.Value]);
                    if (result == "")
                    {
                        await command.RespondWithFileAsync(new FileAttachment(
                            new MemoryStream(Xibao.Render(false, (string)content.Value)), "beibao.png"));
                        break;
                    }

                    await command.RespondAsync(result);
                    break;
                }
                case "扔云瓶":
                {
                    SocketSlashCommandDataOption? content = command.Data.Options.FirstOrDefault();
                    ThrowDriftbottle throwDriftbottle = Commands.GetHarmonyCommand<ThrowDriftbottle>();
                    await command.RespondAsync(throwDriftbottle.Invoke(EventType.GroupMessage, userMention, userId,
                        ["扔云瓶", (string)content.Value]));
                    break;
                }
                case "捞云瓶":
                {
                    SocketSlashCommandDataOption? content = command.Data.Options.FirstOrDefault();
                    PickDriftbottle pickDriftbottle = Commands.GetHarmonyCommand<PickDriftbottle>();
                    string result = pickDriftbottle.Invoke(EventType.GroupMessage, userMention, userId,
                        ["捞云瓶", content == null ? int.MinValue.ToString() : ((long)content.Value).ToString()]);
                    if (result.Contains('\u2408'))
                    {
                        string reply = "";
                        List<string> images = [];
                        int pos = 0;
                        for (int i = 0; i < result.Length; i++)
                        {
                            switch (result[i])
                            {
                                case '\u2408':
                                {
                                    reply += result.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                                    int end = result.IndexOf('\u2409', i + 1);
                                    images.Add(result.Substring(i + 1, end - i - 1));
                                    i = pos = end;
                                    continue;
                                }
                            }
                        }

                        if (pos < result.Length - 1) reply += result[(pos + (result[pos + 1] == ' ' ? 2 : 1))..];

                        await command.RespondWithFilesAsync(
                            images.ConvertAll(path => new FileAttachment(path, path)),
                            reply);
                        break;
                    }

                    await command.RespondAsync(result);
                    break;
                }
                default:
                {
                    IHarmonyCommand? harmony = Commands.GetHarmonyCommand<IHarmonyCommand>(command.CommandName);
                    if (harmony is not null)
                    {
                        await command.RespondAsync(harmony.Invoke(EventType.GroupMessage, userMention, userId, []));
                    }
                    else
                    {
                        IGeneralCommand? general =
                            Commands.GetGeneralCommand<IGeneralCommand>(Platform.Discord, command.CommandName);
                        if (general is not null)
                        {
                            await command.RespondAsync(general.DiscordInvoke(EventType.GroupMessage, userMention,
                                userId, []));
                        }
                        else
                        {
                            await command.RespondAsync("未知命令。请使用 ``/help`` 查看命令列表。");
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