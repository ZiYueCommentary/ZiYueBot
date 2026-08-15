namespace ZiYueBot;

using System.Text;

internal static class Program
{
    // private static readonly ILog Logger = LogManager.GetLogger("入口点");

    private static void InitLogger()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        // log4net.Config.XmlConfigurator.Configure();
    }

    private static void InitDirectories()
    {
        Directory.CreateDirectory("data");
        Directory.CreateDirectory("temp");
        Directory.CreateDirectory("data/images");
    }

    public static async Task Start()
    {
        // InitLogger();
        InitDirectories();

        ZiYueBot bot = ZiYueBot.Create();

        try
        {
            await bot.StartAsync();
            await bot.WaitAsync();
        }
        catch (Exception e)
        {
            // Logger.Fatal("主程序意外退出", e);
        }
    }
}