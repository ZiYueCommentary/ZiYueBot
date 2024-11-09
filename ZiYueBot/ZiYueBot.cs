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
    public static readonly string Version = "0.0.1";
    
    public static readonly ILog LoggerDiscord = LogManager.GetLogger("Discord 主程序");
    public static readonly ILog LoggerQQ = LogManager.GetLogger("QQ 主程序");
    public static ZiYueBot Instance;
    
    public readonly BotContext QQ;
    public readonly DiscordSocketClient Discord;
    private BotDeviceInfo _deviceInfo;
    private BotKeystore _keystore;
    private readonly bool _canAutoLogin;
    private readonly Config _discordConfig;

    private ZiYueBot()
    {
        _canAutoLogin = DeserializeBotConfigs();
        QQ = BotFactory.Create(new BotConfig(), _deviceInfo, _keystore);
        LoggerQQ.Info("初始化完毕");
        using (FileStream stream = new FileStream("config.json", FileMode.OpenOrCreate, FileAccess.Read))
        {
            _discordConfig = JsonSerializer.Deserialize<Config>(stream);
        }

        WebProxy proxy = new WebProxy(_discordConfig.Proxy)
        {
            Credentials = new NetworkCredential(_discordConfig.Username, _discordConfig.Password)
        };
        Discord = new DiscordSocketClient(new DiscordSocketConfig
        {
            //todo
            //WebSocketProvider = DefaultWebSocketProvider.Create(proxy)
        });
        LoggerDiscord.Info("初始化完毕");
    }

    private bool DeserializeBotConfigs()
    {
        bool hasKeystore = false;
        bool hasDeviceInfo = false;
        try
        {
            using (FileStream stream = new FileStream("data/keystore.json", FileMode.OpenOrCreate, FileAccess.Read))
            {
                _keystore = JsonSerializer.Deserialize<BotKeystore>(stream);
            }

            if (_keystore is null) throw new NullReferenceException();
            hasKeystore = true;
        }
        catch (Exception e) when (e is JsonException or NullReferenceException)
        {
            LoggerQQ.Warn("未找到data/keystore.json");
            _keystore = new BotKeystore();
        }

        try
        {
            using (FileStream stream = new FileStream("data/deviceinfo.json", FileMode.OpenOrCreate, FileAccess.Read))
            {
                _deviceInfo = JsonSerializer.Deserialize<BotDeviceInfo>(stream);
            }

            if (_deviceInfo is null) throw new NullReferenceException();
            hasDeviceInfo = true;
        }
        catch (Exception e) when (e is JsonException or NullReferenceException)
        {
            LoggerQQ.Warn("未找到data/deviceinfo.json");
            _deviceInfo = new BotDeviceInfo()
            {
                Guid = new Guid(),
                DeviceName = "ZiYueBot Service",
                SystemKernel = "Windows 10.0.19042",
                KernelVersion = "10.0.19042.0"
            };
        }

        return hasKeystore && hasDeviceInfo;
    }

    private void SerializeBotConfigs()
    {
        File.WriteAllText("data/keystore.json", JsonSerializer.Serialize(_keystore));
        File.WriteAllText("data/deviceinfo.json", JsonSerializer.Serialize(_deviceInfo));
    }

    private async void Login()
    {
        if (_canAutoLogin)
        {
            LoggerQQ.Info("正在进行自动登录...");
            await QQ.LoginByPassword();
            _keystore = QQ.UpdateKeystore();
        }
        else
        {
            LoggerQQ.Info("正在进行扫码登录...");
            var qrCode = await QQ.FetchQrCode();
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData data = qrGenerator.CreateQrCode(qrCode.Value.Url, QRCodeGenerator.ECCLevel.Q);
            AsciiQRCode image = new AsciiQRCode(data);
            foreach (string graph in image.GetLineByLineGraphic(1))
            {
                Console.WriteLine(graph);
            }

            await QQ.LoginByQrCode();
        }

        SerializeBotConfigs();
        LoggerQQ.Info("登录成功！");
        Events.Initialize();

        await Discord.LoginAsync(TokenType.Bot, _discordConfig.Token);
        await Discord.StartAsync();
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