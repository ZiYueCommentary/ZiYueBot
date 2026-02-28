namespace ZiYueBot.Utils;

using global::ZiYueBot.Core;

public static class DiscordUtils
{
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