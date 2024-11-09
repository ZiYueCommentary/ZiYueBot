using log4net;
using ZiYueBot.Core;

namespace ZiYueBot.Harmony;

public class BALogo : IHarmonyCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("碧蓝档案标题");


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
               在线文档：https://docs.ziyuebot.cn/balogo.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "生成《碧蓝档案》标题";
    }

    public string Invoke(EventType type, string userName, ulong userId, string[] args)
    {
        throw new NotImplementedException();
    }

    public void Render(string left, string right)
    {
        //Graphics graphics = new Graphics(new PngImage());
        //graphics.DrawString();
    }
}