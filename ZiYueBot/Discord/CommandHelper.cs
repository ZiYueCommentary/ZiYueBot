using Discord;
using Discord.Net;
using Discord.WebSocket;
using ZiYueBot.Core;

namespace ZiYueBot.Discord;

public static class CommandHelper
{
    public static async Task RegisterCommand(SlashCommandBuilder builder)
    {
        try
        {
            try
            {
                await ZiYueBot.Instance.Discord.CreateGlobalApplicationCommandAsync(builder.Build());
            }
            catch (HttpRequestException e)
            {
                DiscordHandler.Logger.Error("无法连接 Discord 服务器！", e);
            }
            catch (TimeoutException)
            {
                DiscordHandler.Logger.Warn("连接超时");
            }
        }
        catch (HttpException e)
        {
            if (e.HttpCode == System.Net.HttpStatusCode.MethodNotAllowed)
            {
                DiscordHandler.Logger.Warn($"命令重复注册：{builder.Name}");
            }
            else
            {
                DiscordHandler.Logger.Error($"命令注册失败：{builder.Name}", e);
            }
        }
    }

    public static void AddCommandsAsChoices(SlashCommandOptionBuilder builder)
    {
        foreach (Command command in Commands.RegisteredCommands.Values.ToHashSet()
                     .Where(general => general.SupportedPlatform.Contains(Platform.Discord)))
        {
            builder.AddChoice($"{command.Name}（{command.Id}）", command.Id);
        }
    }

    public static SlashCommandBuilder EasyCommandBuilder(Command command)
    {
        SlashCommandBuilder builder = new SlashCommandBuilder();
        builder.WithName(command.Id);
        builder.WithDescription(command.Summary);
        return builder;
    }
}