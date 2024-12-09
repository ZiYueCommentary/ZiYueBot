﻿using Discord;
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
using Discord.Net.Rest;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot;

public class ZiYueBot
{
    public const string Version = "0.0.1";

    private static readonly ILog Logger = LogManager.GetLogger("主程序");
    public static ZiYueBot Instance;

    public readonly BotContext QQ;
    public readonly DiscordSocketClient Discord;

    private BotDeviceInfo _deviceInfo;
    private BotKeystore _keystore;
    private readonly bool _canAutoLogin;
    private readonly Config _config;

    private ZiYueBot()
    {
        _canAutoLogin = DeserializeBotConfigs();
        QQ = BotFactory.Create(new BotConfig(), _deviceInfo, _keystore);
        Logger.Info("QQ - 初始化完毕");
        using (FileStream stream = new FileStream("config.json", FileMode.OpenOrCreate, FileAccess.Read))
        {
            _config = JsonSerializer.Deserialize<Config>(stream);
        }
        
        Discord = new DiscordSocketClient(new DiscordSocketConfig
        {
            RestClientProvider = DefaultRestClientProvider.Create(true),
            WebSocketProvider = DefaultWebSocketProvider.Create(new WebProxy("http://127.0.0.1:7890"))
        });
        Logger.Info("Discord - 初始化完毕");

        InitializeDatabase();
        Logger.Info("MySQL - 初始化完毕");
    }

    private void InitializeDatabase()
    {
        using MySqlConnection database = ConnectDatabase();

        try
        {
            MySqlCommand command = new MySqlCommand("""
                                                    CREATE TABLE driftbottles
                                                    (
                                                        id       int auto_increment primary key,
                                                        userid   bigint                    null,
                                                        username tinytext                  null,
                                                        created  datetime                  null,
                                                        content  text                      null,
                                                        pickable boolean           default true,
                                                        views    int                  default 0
                                                    ) CHARSET = utf8mb4;
                                                    """, database);
            command.ExecuteNonQuery();
        }
        catch (MySqlException)
        {
        }

        try
        {
            MySqlCommand command = new MySqlCommand("""
                                                    CREATE TABLE straitbottles
                                                    (
                                                        id            int auto_increment primary key,
                                                        userid        bigint                    null,
                                                        username      tinytext                  null,
                                                        created       datetime                  null,
                                                        content       text                      null,
                                                        fromDiscord   boolean                   null,
                                                        picked        boolean          default false
                                                    ) CHARSET = utf8mb4;
                                                    """, database);
            command.ExecuteNonQuery();
        }
        catch (MySqlException)
        {
        }

        try
        {
            MySqlCommand command = new MySqlCommand("""
                                                    create table win
                                                    (
                                                        userid      bigint            unique,
                                                        username    tinytext            null,
                                                        channel     bigint              null,
                                                        date        date                null,
                                                        score       tinyint             null,
                                                        prospered   boolean    default false,
                                                        miniWinDays tinyint        default 0
                                                    ) CHARSET = utf8mb4;
                                                    """, database);
            command.ExecuteNonQuery();
        }
        catch (MySqlException)
        {
        }
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
            Logger.Warn("QQ - 未找到data/keystore.json");
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
            Logger.Warn("QQ - 未找到data/deviceinfo.json");
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
        try
        {
            if (_canAutoLogin)
            {
                Logger.Info("QQ - 正在进行自动登录...");
                await QQ.LoginByPassword();
                _keystore = QQ.UpdateKeystore();
            }
            else
            {
                Logger.Info("QQ - 正在进行扫码登录...");
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
            Logger.Info("QQ - 登录成功！");
            Events.Initialize();

            await Discord.LoginAsync(TokenType.Bot, _config.DiscordToken);
            await Discord.StartAsync();
            Logger.Info("Discord - 登录成功！");
            Handler.Initialize();
        }
        catch (Exception e)
        {
            Logger.Error(e.Message, e);
        }
    }

    public MySqlConnection ConnectDatabase()
    {
        MySqlConnection connection = new MySqlConnection(
            $"""
             server={_config.DatabaseSource};
             port={_config.DatabasePort};
             database={_config.DatabaseName};
             user={_config.DatabaseUser};
             password={_config.DatabasePassword};
             charset=utf8mb4;
             allowuservariables=True;
             """
        );
        connection.Open();
        return connection;
    }

    public static void Main()
    {
        //System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        log4net.Config.XmlConfigurator.Configure();
        Directory.CreateDirectory("data");
        Directory.CreateDirectory("temp");
        Directory.CreateDirectory("data/images");
        Instance = new ZiYueBot();
        Instance.Login();
        Commands.Initialize();
    }
}