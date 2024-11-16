using log4net;
using SkiaSharp;
using ZiYueBot.Core;
using ZiYueBot.General;
using ZiYueBot.Utils;

namespace ZiYueBot.Harmony;

public class BALogo : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("碧蓝档案标题");
    private static readonly double OffsetX = 250 / Math.Tan(double.DegreesToRadians(60));
    private static readonly SKTypeface Face = SKTypeface.FromFile("resources/BlueArchive.ttf");
    private static readonly SKFont Font = new SKFont(Face, 84);

    public string GetCommandId()
    {
        return "balogo";
    }

    public string GetCommandName()
    {
        return "碧蓝档案标题";
    }

    public string GetCommandDescription()
    {
        return """
               /balogo [left] [right]
               生成《碧蓝档案》风格的标题图片。“left”为光环左侧的文字，“right”为光环右侧的文字。
               如两侧的文本中包含空格，请将文本用英文引号包裹。
               频率限制：每次调用间隔 1 分钟。
               在线文档：https://docs.ziyuebot.cn/balogo.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "生成《碧蓝档案》标题";
    }

    public string Invoke(EventType eventType, string userName, ulong userId, string[] args)
    {
        if (args.Length < 3) return "参数数量不足。使用 “/help balogo” 查看命令用法。";
        if (!MessageUtils.IsSimpleMessage(args[0]) || !MessageUtils.IsSimpleMessage(args[1])) return "请输入纯文字参数。";
        if (!RateLimit.TryPassRateLimit(this, eventType, userId)) return "频率已达限制（每分钟 1 条）";

        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");
        return "";
    }

    public byte[] Render(string left, string right)
    {
        float leftWidth = Font.MeasureText(left);
        float rightWidth = Font.MeasureText(right);
        float width = (float)(Math.Max(leftWidth, rightWidth) * 2D + OffsetX);
        using SKSurface? surface = SKSurface.Create(new SKImageInfo((int)width, 250));
        SKCanvas? canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        using SKPaint haloPaint = new SKPaint();
        haloPaint.IsAntialias = true;
        canvas.Save();
        SKMatrix matrix = SKMatrix.CreateSkew(-0.5F, 0);
        canvas.Concat(in matrix);
        haloPaint.Color = new SKColor(18, 138, 250);
        canvas.DrawText(left, (float)((width + OffsetX) / 2) - (rightWidth - leftWidth) / 2, (float)(250 * 0.68),
            SKTextAlign.Right, Font, haloPaint);
        haloPaint.Color = new SKColor(43, 43, 43);
        canvas.DrawText(right, (float)((width + OffsetX) / 2) - (rightWidth - leftWidth) / 2, (float)(250 * 0.68),
            SKTextAlign.Left, Font, haloPaint);
        canvas.Restore();
        using SKBitmap halo = SKBitmap.Decode("resources/halo.png");
        canvas.DrawBitmap(halo, (float)((width - OffsetX) / 2) - (rightWidth - leftWidth) / 2, 0, haloPaint);
        using SKData? output = surface.Snapshot().Encode();
        return output.ToArray();
    }

    public TimeSpan GetRateLimit(Platform platform, EventType eventType)
    {
        return eventType == EventType.GroupMessage ? TimeSpan.FromMinutes(1) : TimeSpan.Zero;
    }
}