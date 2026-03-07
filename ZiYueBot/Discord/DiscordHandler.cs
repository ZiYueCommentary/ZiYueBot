using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using log4net;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Harmony;
using ZiYueBot.Utils;

namespace ZiYueBot.Discord;

public static class DiscordHandler
{
    internal static readonly ILog Logger = LogManager.GetLogger("Discord 消息解析");

    public static void Initialize()
    {
        ZiYueBot.Instance.Discord.Ready += OnReady;
        ZiYueBot.Instance.Discord.SlashCommandExecuted += OnSlashCommandExecuted;
        ZiYueBot.Instance.Discord.ReactionAdded += OnReactionAdded;
    }

    private static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> user,
        Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        try
        {
            IMessage message = await channel.Value.GetMessageAsync(reaction.MessageId);
            if (message.Author.Id == 1189195615083704422)
            {
                Match match = Stargazers.StargazerRegex().Match(message.Content.FirstLine());
                if (match.Success && reaction.Emote.Name is "👍" or "⭐")
                {
                    string result = Stargazers.AddStargazer(reaction.UserId,
                        reaction.User.Value.Mention, int.Parse(match.Groups[1].Value), true);
                    if (!string.IsNullOrEmpty(result)) await message.Channel.SendMessageAsync(result);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e.Message, e);
        }
    }

    private static async Task OnReady()
    {
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new Jrrp()));
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new Hitokoto()));
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new About()));
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new Quotations()));
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new ListDriftbottle()));
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new PickStraitbottle()));
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new ListStraitbottle()));
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new StartRevolver()));
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new RestartRevolver()));
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new Rotating()));
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new Win()));
        await CommandHelper.RegisterCommand(CommandHelper.EasyCommandBuilder(new Stat()));
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new Chat());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("question").WithDescription("问题内容").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new Draw());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("prompt").WithDescription("正向提示词").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new Shooting());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("user").WithDescription("开枪目标").WithRequired(false)
                .WithType(ApplicationCommandOptionType.User);
            builder.AddOption(optionBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new ThrowStraitbottle());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("content").WithDescription("瓶子内容").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new ThrowDriftbottle());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("content").WithDescription("瓶子内容").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new PickDriftbottle());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("id").WithDescription("瓶子编号").WithRequired(false)
                .WithType(ApplicationCommandOptionType.Integer);
            builder.AddOption(optionBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new RemoveDriftbottle());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("id").WithDescription("瓶子编号").WithRequired(true)
                .WithType(ApplicationCommandOptionType.Integer);
            builder.AddOption(optionBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new Help());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("command").WithDescription("获取帮助的命令名").WithRequired(false)
                .WithType(ApplicationCommandOptionType.String);
            CommandHelper.AddCommandsAsChoices(optionBuilder);
            builder.AddOption(optionBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new Ask());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("question").WithDescription("向张维为教授提出问题").WithRequired(false)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new BaLogo());
            SlashCommandOptionBuilder optionLeftBuilder = new SlashCommandOptionBuilder();
            optionLeftBuilder.WithName("left").WithDescription("光环左侧文字").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            SlashCommandOptionBuilder optionRightBuilder = new SlashCommandOptionBuilder();
            optionRightBuilder.WithName("right").WithDescription("光环右侧文字").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOptions(optionLeftBuilder, optionRightBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new Xibao());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("content").WithDescription("喜报内容").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
        {
            SlashCommandBuilder builder = CommandHelper.EasyCommandBuilder(new Beibao());
            SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder();
            optionBuilder.WithName("content").WithDescription("悲报内容").WithRequired(true)
                .WithType(ApplicationCommandOptionType.String);
            builder.AddOption(optionBuilder);
            await CommandHelper.RegisterCommand(builder);
        }
    }

    private static async Task OnSlashCommandExecuted(SocketSlashCommand command)
    {
        try
        {
            DiscordContext context =
                new DiscordContext(command.GuildId is null ? EventType.DirectMessage : EventType.GroupMessage,
                    command.User.GlobalName, command.User.Id, command);

            if (Commands.GetCommand(Platform.Discord, command.CommandName) is null)
            {
                await command.RespondAsync("未知命令。请使用 /help 查看命令列表。");
                return;
            }

            if (await Commands.CheckBlacklist(context, command.CommandName)) return;

            IReadOnlyCollection<SocketSlashCommandDataOption> options = command.Data.Options;
            MessageChain arg = [];

            foreach (SocketSlashCommandDataOption option in options)
            {
                switch (option.Type)
                {
                    case ApplicationCommandOptionType.User:
                        arg.Add(new PingMessageEntity(((IUser)option.Value).Id));
                        break;
                    default:
                        arg.Add(new TextMessageEntity(option.Value.ToString()!));
                        break;
                }
            }

            try
            {
                await Commands.GetCommand(Platform.Discord, command.CommandName)!.Invoke(context, arg);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                await context.SendMessage("命令内部错误。");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message, ex);
            await command.RespondAsync("命令解析错误。");
        }
    }
}