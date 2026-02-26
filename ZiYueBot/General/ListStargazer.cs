using ZiYueBot.Core;

namespace ZiYueBot.General;

public class ListStargazer : Command
{
    public override string Id => "查看星标云瓶";
    public override string Name => "查看星标云瓶";
    public override string Summary => "查看星标云瓶";
    public override string Description => "";

    //TODO 完成星标云瓶查询功能
    public override Task Invoke(IContext context, MessageChain arg)
    {
        throw new NotImplementedException();//占位
    }
}