using Discord;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using ZiYueBot.Discord;
using ZiYueBot.QQ;
using log4net;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Discord.Net.Rest;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot;

public class ZiYueBot
{
    private static readonly ILog Logger = LogManager.GetLogger("主程序");
    public static ZiYueBot Instance { get; private set; }

    public readonly ClientWebSocket QqEvent;
    public readonly ClientWebSocket QqApi;
    public uint QqUserId { get; private set; }
    public readonly DiscordSocketClient Discord;

    public readonly Config Config;

    private ZiYueBot()
    {
        using (FileStream stream = new FileStream("config.json", FileMode.OpenOrCreate, FileAccess.Read))
        {
            Config = JsonSerializer.Deserialize<Config>(stream);
        }

        QqEvent = new ClientWebSocket();
        QqApi = new ClientWebSocket();
        ConnectQqWebSocket();

        Discord = new DiscordSocketClient(new DiscordSocketConfig
        {
            RestClientProvider = DefaultRestClientProvider.Create(true),
            WebSocketProvider =
                DefaultWebSocketProvider.Create(new WebProxy(Environment.GetEnvironmentVariable("HTTPS_PROXY")))
        });
        Discord.LoginAsync(TokenType.Bot, Config.DiscordToken).Wait();
        Discord.StartAsync().Wait();
        // Logger.Info($"Discord 登录成功：{Discord.CurrentUser.GlobalName} ({Discord.CurrentUser.Id})");

        InitializeDatabase();
    }

    internal void ConnectQqWebSocket()
    {
        QqEvent.Options.SetRequestHeader("Authorization", "Bearer " + Config.QqEventAuthenticate);
        QqEvent.ConnectAsync(new Uri(Config.QqEventEndpoint), CancellationToken.None).Wait();
        QqApi.Options.SetRequestHeader("Authorization", "Bearer " + Config.QqApiAuthenticate);
        QqApi.ConnectAsync(new Uri(Config.QqApiEndpoint), CancellationToken.None).Wait();
        QqApi.SendAsync(new ArraySegment<byte>("{\"action\": \"get_login_info\"}"u8.ToArray()),
            WebSocketMessageType.Text, true, CancellationToken.None);
        byte[] buffer = new byte[4096];
        WebSocketReceiveResult result = QqApi.ReceiveAsync(new ArraySegment<byte>(buffer),
            CancellationToken.None).GetAwaiter().GetResult();
        JsonNode qqUserInfo = JsonNode.Parse(Encoding.UTF8.GetString(buffer, 0, result.Count))!;
        QqUserId = qqUserInfo["data"]!["user_id"]!.GetValue<uint>();
        Logger.Info($"QQ 连接成功：{qqUserInfo["data"]!["nickname"]!.GetValue<string>()} ({QqUserId})");
    }

    private void InitializeDatabase()
    {
        try
        {
            using FileStream stream = new FileStream("resources/initialize.sql", FileMode.OpenOrCreate);
            using StreamReader reader = new StreamReader(stream);
            MySqlCommand command = new MySqlCommand(reader.ReadToEnd(), ConnectDatabase());
            command.ExecuteNonQuery();
            Logger.Info("数据库初始化成功");
        }
        catch (Exception e)
        {
            Logger.Error("数据库初始化出错", e);
        }
    }

    public MySqlConnection ConnectDatabase()
    {
        MySqlConnection connection = new MySqlConnection(
            $"""
             Server={Config.DatabaseSource};
             Port={Config.DatabasePort};
             Database={Config.DatabaseName};
             User={Config.DatabaseUser};
             Password={Config.DatabasePassword};
             Charset=utf8mb4;
             AllowUserVariables=True;
             Pooling=true;
             """
        );
        connection.Open();
        return connection;
    }

    public static void Main()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        log4net.Config.XmlConfigurator.Configure();
        Directory.CreateDirectory("data");
        Directory.CreateDirectory("temp");
        Directory.CreateDirectory("data/images");
        Commands.Initialize();
        Instance = new ZiYueBot();
        DiscordHandler.Initialize();
        QqEvents.Initialize().Wait();
    }
}