using log4net;
using SkiaSharp;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Utils;

namespace ZiYueBot.Harmony;

public class Xibao : IHarmonyCommand
{
    private static readonly SKFont Font = new SKFont(SKTypeface.FromFile("resources/HarmonyOS.ttf"), 100);
    private static readonly SKBitmap ImageXibao = SKBitmap.Decode("resources/xibao.jpg");
    private static readonly SKBitmap ImageBeibao = SKBitmap.Decode("resources/beibao.jpg");
    private static readonly ILog Logger = LogManager.GetLogger("喜报");
    
    public string GetCommandId()
    {
        return "xibao";
    }

    public string GetCommandName()
    {
        return "喜报";
    }

    public string GetCommandDescription()
    {
        return """
               /xibao
               生成一张喜报。
               频率限制：每次调用间隔 1 分钟。
               在线文档：https://docs.ziyuebot.cn/xibao.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "生成一张喜报";
    }

    public string Invoke(EventType type, string userName, ulong userId, string[] args)
    {
        if (args.Length < 2) return "参数数量不足。使用 “/help xibao” 查看命令用法。";
        if (!MessageUtils.IsSimpleMessage(args[0]) || !MessageUtils.IsSimpleMessage(args[1])) return "请输入纯文字参数。";
        if (!RateLimit.TryPassRateLimit(this, EventType.GroupMessage, userId)) return "频率已达限制（每分钟 1 条）";
        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        return "";
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return TimeSpan.FromMinutes(1);
    }

    public static byte[] Render(bool isXibao, string text)
    {
        using SKSurface? surface = SKSurface.Create(new SKImageInfo(1024, 768));
        SKCanvas? canvas = surface.Canvas;
        SKPaint paint = new SKPaint()
        {
            IsAntialias = true,
            Color = isXibao ? new SKColor(255, 10, 10) : new SKColor(0, 5, 0)
        };
        SKBitmap image = isXibao ? ImageXibao : ImageBeibao;
        canvas.DrawBitmap(image, 0, 0, paint);
        DrawCenteredText(canvas, text, 1024, 512, 384, Font, paint);
        using SKData? output = surface.Snapshot().Encode();
        return output.ToArray();
    }

    public static void DrawCenteredText(SKCanvas canvas, string text, float width, float x, float y, SKFont font, SKPaint paint)
    {
        List<string> lines = [];
        while (!string.IsNullOrEmpty(text))
        {
            if (text.StartsWith('\r') || text.StartsWith('\n'))
            {
                text = text[1..];
                continue;
            }

            int breakIndex = font.BreakText(text, width, out _);
            int rIndex = text.IndexOf('\r');
            int nIndex = text.IndexOf('\n');
            rIndex = rIndex == -1 ? int.MaxValue : rIndex;
            nIndex = nIndex == -1 ? int.MaxValue : nIndex;
            int feedIndex = Math.Min(rIndex, nIndex);

            if (feedIndex < breakIndex)
            {
                lines.Add(text[..feedIndex]);
                text = text[(feedIndex + 1)..];
            }
            else
            {
                lines.Add(text[..breakIndex]);
                text = text[breakIndex..];
            }
        }

        float height = lines.Count * font.Spacing;
        float baselineY = y - height / 2 - font.Metrics.Ascent;
        foreach (string line in lines)
        {
            canvas.DrawText(line, x, baselineY, SKTextAlign.Center, font, paint);
            baselineY += font.Spacing;
        }
    }
}