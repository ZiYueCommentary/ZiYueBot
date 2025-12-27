using Discord;
using Discord.Net;
using Discord.WebSocket;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Harmony;

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
        foreach (HarmonyCommand harmony in Commands.HarmonyCommands.Values.ToHashSet())
        {
            builder.AddChoice($"{harmony.Name} ({harmony.Id})", harmony.Id);
        }

        foreach (GeneralCommand general in Commands.GeneralCommands.Values.ToHashSet().Where(general =>
                     general.SupportedPlatform.Contains(Platform.Discord)))
        {
            builder.AddChoice($"{general.Name}（{general.Id}）", general.Id);
        }
    }

    public static SlashCommandBuilder EasyCommandBuilder(Command command)
    {
        SlashCommandBuilder builder = new SlashCommandBuilder();
        builder.WithName(command.Id);
        builder.WithDescription(command.Summary);
        return builder;
    }

    public static async Task SendComplexMessage(SocketSlashCommand command, string message)
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
}