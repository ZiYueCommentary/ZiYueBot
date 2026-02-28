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

    // Author: EasyT_T
    public static void ParseRawMessage(ReadOnlySpan<char> text, MessageChain messageChain)
    {
        int index = 0;
        int prevPingEnd = 0;

        while (index < text.Length)
        {
            int savedIndex = index;

            if (!EatChar(text, '<'))
            {
                index = savedIndex + 1;

                continue;
            }

            if (!EatChar(text, '@'))
            {
                index = savedIndex + 1;

                continue;
            }

            if (!EatDigits(text, out int start, out int end))
            {
                index = savedIndex + 1;

                continue;
            }

            int pingStart = start - 2;

            if (!EatChar(text, '>'))
            {
                index = savedIndex + 1;

                continue;
            }

            if (!ulong.TryParse(text[start..end], out ulong userId))
            {
                index = savedIndex + 1;

                continue;
            }

            // Add previous TextMessageEntity before adding PingMessageEntity if it has
            if (pingStart - prevPingEnd > 0)
            {
                messageChain.Add(new TextMessageEntity(text[prevPingEnd..pingStart].ToString()));
            }

            prevPingEnd = index;

            messageChain.Add(new PingMessageEntity(userId));
        }

        // Add TextMessage after PingMessageEntity if it has
        if (index - prevPingEnd > 0)
        {
            messageChain.Add(new TextMessageEntity(text[prevPingEnd..index].ToString()));
        }

        return;

        bool EatChar(ReadOnlySpan<char> span, char c)
        {
            if (index >= span.Length)
            {
                return false;
            }

            if (span[index] != c)
            {
                return false;
            }

            index++;

            return true;
        }

        bool EatDigits(ReadOnlySpan<char> span, out int start, out int end)
        {
            if (index >= span.Length || !char.IsAsciiDigit(span[index]))
            {
                start = -1;
                end = -1;

                return false;
            }

            start = index;

            while (index < span.Length)
            {
                if (!char.IsAsciiDigit(span[index]))
                {
                    break;
                }

                index++;
            }

            end = index;

            return true;
        }
    }
}