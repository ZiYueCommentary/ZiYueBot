using log4net;
using SkiaSharp;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.Harmony;

public class Xibao : Command
{
    private static readonly SKFont Font = new SKFont(SKTypeface.FromFile("resources/HarmonyOS.ttf"), 100);
    private static readonly SKBitmap ImageXibao = SKBitmap.Decode("resources/xibao.jpg");
    private static readonly SKBitmap ImageBeibao = SKBitmap.Decode("resources/beibao.jpg");
    private static readonly ILog Logger = LogManager.GetLogger("喜报");

    public override string Id => "xibao";

    public override string Name => "喜报";

    public override string Summary => "生成一张喜报";

    public override string Description => """
                                          /xibao [content]
                                          生成一张喜报。“content”是喜报的内容，必须为纯文字。
                                          频率限制：每次调用间隔 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/harmony/xibao
                                          """;

    public override async Task Invoke(IContext context, MessageChain arg)
    {
        if (arg.IsEmpty())
        {
            await context.SendMessage("参数数量不足。使用 “/help xibao” 查看命令用法。");
            return;
        }

        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage("频率已达限制（每分钟 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);

        if (!arg.IsLiteralString())
        {
            await context.SendMessage("请输入纯文字参数。");
            return;
        }

        await context.SendMessage("机器生成中...");

        await context.SendMessage([
            new ImageMessageEntity($"base64://{Convert.ToBase64String(Render(true, arg.ToString(context)))}", "xibao.jpg")
        ]);
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
        using SKData? output = surface.Snapshot().Encode(SKEncodedImageFormat.Jpeg, 90);
        return output.ToArray();
    }

    private static void DrawCenteredText(SKCanvas canvas, string text, float width, float x, float y, SKFont font,
        SKPaint paint)
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

    public override TimeSpan GetRateLimit(IContext context)
    {
        return TimeSpan.FromMinutes(1);
    }
}