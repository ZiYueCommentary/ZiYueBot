using System.Text.Json.Nodes;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.QQ;

public static class Parser
{
    public static MessageChain ParseMessage(JsonNode node, out MessageChain? forwardMessage, QqContext? context = null)
    {
        MessageChain chain = [];
        forwardMessage = null;
        foreach (JsonNode? jsonNode in node.AsArray())
        {
            JsonObject? segment = (JsonObject?)jsonNode;
            switch (segment!["type"]!.GetValue<string>())
            {
                case "reply":
                {
                    if (context is not null)
                        forwardMessage = context.FetchMessageContent(segment["data"]!["id"]!.GetValue<string>(), out _);
                    break;
                }
                case "text":
                {
                    chain.Add(new TextMessageEntity(segment["data"]!["text"]!.GetValue<string>().SafeArgument()));
                    break;
                }
                case "image":
                {
                    chain.Add(new ImageMessageEntity(segment["data"]!["url"]!.GetValue<string>(), ""));
                    break;
                }
                case "at":
                {
                    string qq = segment["data"]!["qq"]!.GetValue<string>();
                    chain.Add(new PingMessageEntity(qq == "all" ? 0 : ulong.Parse(qq)));
                    break;
                }
            }
        }

        return chain;
    }
}