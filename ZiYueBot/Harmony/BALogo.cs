using System.CommandLine.Parsing;
using log4net;
using SkiaSharp;
using ZiYueBot.Core;

namespace ZiYueBot.Harmony;

public class BaLogo : Command
{
    private static readonly ILog Logger = LogManager.GetLogger("碧蓝档案标题");
    private static readonly double OffsetX = 250 / Math.Tan(double.DegreesToRadians(60));
    private static readonly SKTypeface Face = SKTypeface.FromFile("resources/BlueArchive.ttf");
    private static readonly SKFont Font = new SKFont(Face, 84);

    public override string Id => "balogo";

    public override string Name => "碧蓝档案标题";

    public override string Summary => "生成《碧蓝档案》标题";

    public override string Description => """
                                          /balogo [left] [right]
                                          生成《碧蓝档案》风格的标题图片。“left”为光环左侧的文字，“right”为光环右侧的文字。
                                          如两侧的文本中包含空格，请将文本用英文引号包裹。
                                          频率限制：每次调用间隔 1 分钟。
                                          在线文档：https://docs.ziyuebot.cn/harmony/balogo
                                          """;

    public override async Task Invoke(Context context, MessageChain arg)
    {
        if (!arg.IsLiteralString())
        {
            await context.SendMessage("请输入纯文字参数。");
            return;
        }

        List<string> args = context.Platform == Platform.QQ
            ? CommandLineParser.SplitCommandLine(arg.ToString(context)).ToList()
            : [arg[0].ToString(context), arg[1].ToString(context)];

        if (args.Count < 2)
        {
            await context.SendMessage("参数数量不足。使用“/help balogo”查看命令用法。");
            return;
        }

        if (!this.TryPassRateLimit(context))
        {
            await context.SendMessage("频率已达限制（每分钟 1 条）");
            return;
        }

        Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg}");
        _ = UpdateInvokeRecords(context.UserId);

        await context.SendMessage([
            new ImageMessageEntity($"base64://{Convert.ToBase64String(Render(args[0], args[1]))}", "balogo.jpg")
        ]);
    }

    private static byte[] Render(string left, string right)
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
        using SKData? output = surface.Snapshot().Encode(SKEncodedImageFormat.Jpeg, 90);
        return output.ToArray();
    }

    public override TimeSpan GetRateLimit(Context context)
    {
        return TimeSpan.FromMinutes(1);
    }
}