using Discord;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using ZiYueBot.Discord;
using ZiYueBot.QQ;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Interface.Api;
using log4net;
using QRCoder;
using System.Net;
using System.Text.Json;
using ZiYueBot.Core;

namespace ZiYueBot;

public class ZiYueBot
{
    public static ZiYueBot Instance;
    public BotContext QQ;
    protected BotDeviceInfo DeviceInfo;
    protected BotKeystore Keystore;
    private readonly bool CanAutoLogin;
    public static readonly ILog LoggerQQ = LogManager.GetLogger("QQ 主程序");
    public DiscordSocketClient Discord;
    private Config DiscordConfig;
    public static readonly ILog LoggerDiscord = LogManager.GetLogger("Discord 主程序");

    private ZiYueBot()
    {
        this.CanAutoLogin = DeserializeBotConfigs();
        this.QQ = BotFactory.Create(new BotConfig(), this.DeviceInfo, this.Keystore);
        LoggerQQ.Info("初始化完毕");
        using (FileStream stream = new FileStream("config.json", FileMode.OpenOrCreate, FileAccess.Read))
        {
            this.DiscordConfig = JsonSerializer.Deserialize<Config>(stream);
        }
        WebProxy proxy = new WebProxy(this.DiscordConfig.Proxy)
        {
            Credentials = new NetworkCredential(this.DiscordConfig.Username, this.DiscordConfig.Password)
        };
        this.Discord = new DiscordSocketClient(new DiscordSocketConfig
        {
            //WebSocketProvider = DefaultWebSocketProvider.Create(proxy)
        });
        LoggerDiscord.Info("初始化完毕");
    }

    private bool DeserializeBotConfigs()
    {
        bool hasKeystore = false;
        bool hasDeviceinfo = false;
        try
        {
            using (FileStream stream = new FileStream("data/keystore.json", FileMode.OpenOrCreate, FileAccess.Read))
            {
                this.Keystore = JsonSerializer.Deserialize<BotKeystore>(stream);
            }
            if (this.Keystore is null) throw new NullReferenceException();
            hasKeystore = true;
        }
        catch (Exception e) when (e is JsonException or NullReferenceException)
        {
            LoggerQQ.Warn("未找到data/keystore.json");
            this.Keystore = new BotKeystore();
        }

        try
        {
            using (FileStream stream = new FileStream("data/deviceinfo.json", FileMode.OpenOrCreate, FileAccess.Read))
            {
                this.DeviceInfo = JsonSerializer.Deserialize<BotDeviceInfo>(stream);
            }
            if (this.DeviceInfo is null) throw new NullReferenceException();
            hasDeviceinfo = true;
        }
        catch (Exception e) when (e is JsonException or NullReferenceException)
        {
            LoggerQQ.Warn("未找到data/deviceinfo.json");
            this.DeviceInfo = new BotDeviceInfo()
            {
                Guid = new Guid(),
                DeviceName = "ZiYueBot Service",
                SystemKernel = "Windows 10.0.19042",
                KernelVersion = "10.0.19042.0"
            };
        }
        return hasKeystore && hasDeviceinfo;
    }

    private void SerializeBotConfigs()
    {
        File.WriteAllText("data/keystore.json", JsonSerializer.Serialize(Keystore));
        File.WriteAllText("data/deviceinfo.json", JsonSerializer.Serialize(DeviceInfo));
    }

    private async void Login()
    {
        if (CanAutoLogin)
        {
            LoggerQQ.Info("正在进行自动登录...");
            await this.QQ.LoginByPassword();
            this.Keystore = this.QQ.UpdateKeystore();
        } else
        {
            // 假设是初次登录。由于扫码登录耗时，因此在此时可以初始化一些文件。
            WebUtils.DownloadFile("https://eggs.gold/MCA/words.txt", "data/words.txt");

            LoggerQQ.Info("正在进行扫码登录...");
            var qrCode = await this.QQ.FetchQrCode();
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData data = qrGenerator.CreateQrCode(qrCode.Value.Url, QRCodeGenerator.ECCLevel.Q);
            AsciiQRCode image = new AsciiQRCode(data);
            foreach (string graph in image.GetLineByLineGraphic(1))
            {
                Console.WriteLine(graph);
            }
            await this.QQ.LoginByQrCode();
        }
        SerializeBotConfigs();
        LoggerQQ.Info("登录成功！");
        Events.Initialize();

        await this.Discord.LoginAsync(TokenType.Bot, this.DiscordConfig.Token);
        await this.Discord.StartAsync();
        LoggerDiscord.Info("登录成功！");
        Handler.Initialize();
    }

    public static void Main()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); 
        log4net.Config.XmlConfigurator.Configure();
        Directory.CreateDirectory("data");
        Directory.CreateDirectory("temp");
        Instance = new ZiYueBot();
        Instance.Login();
        Commands.Initialize();
    }
}
