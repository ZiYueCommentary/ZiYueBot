using log4net;
using ZiYueBot.Core;

namespace ZiYueBot.Management;

public class Shutdown : PrivilegeCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("停止运行");
    
    public override string Id => "shutdown";
    public override string Name => "停止运行";
    public override string Summary => "管理命令";

    public override string Description => """
                                          /sudo shutdown [reason]（管理命令）
                                          立即停止运行子悦机器。需要 ShutdownService 特权。
                                          “reason”是可选参数，用于记录停止运行的原因。
                                          在线文档：https://docs.ziyuebot.cn/techical/manangement/shutdown
                                          """;
    
    public override Privilege ExpectingPrivileges => Privilege.ShutdownService;
    public override Task PrivilegedInvoke(Context context, MessageChain arg)
    {
        Logger.Info($"调用者：{context.UserName} ({context.UserId})，参数：{arg.Flatten()}");
        _ = UpdateInvokeRecords(context.UserId);
        context.SendMessage("正在停止运行...");
        Environment.Exit(0);
        return Task.CompletedTask;
    }
}