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
    public static ZiYueBot Instance { get; private set; } = null!;

    public ClientWebSocket QqEvent { get; }
    public ClientWebSocket QqApi { get; }
    public uint QqUserId { get; private set; }
    public DiscordSocketClient Discord { get; }

    public readonly Config Config;

    private Task? _eventTask;

    private ZiYueBot()
    {
        using (FileStream stream = File.OpenRead("config.json"))
        {
            Config = JsonSerializer.Deserialize<Config>(stream);
        }

        QqEvent = new ClientWebSocket();
        QqEvent.Options.SetRequestHeader("Authorization", "Bearer " + Config.QqEventAuthenticate);

        QqApi = new ClientWebSocket();
        QqApi.Options.SetRequestHeader("Authorization", "Bearer " + Config.QqApiAuthenticate);

        Discord = new DiscordSocketClient(new DiscordSocketConfig
        {
            RestClientProvider = DefaultRestClientProvider.Create(true),
            WebSocketProvider =
                DefaultWebSocketProvider.Create(new WebProxy(Environment.GetEnvironmentVariable("HTTPS_PROXY")))
        });
    }

    public static ZiYueBot Create()
    {
        Instance = new ZiYueBot();

        DiscordHandler.Initialize();
        Commands.Initialize();

        return Instance;
    }

    private async Task ConnectQqWebSocketAsync()
    {
        await Task.WhenAll(
            QqEvent.ConnectAsync(new Uri(Config.QqEventEndpoint), CancellationToken.None),
            QqApi.ConnectAsync(new Uri(Config.QqApiEndpoint), CancellationToken.None));

        await QqApi.SendAsync("{\"action\": \"get_login_info\"}"u8.ToArray(),
            WebSocketMessageType.Text, true, CancellationToken.None);

        Memory<byte> buffer = new byte[4096];
        ValueWebSocketReceiveResult result = await QqApi.ReceiveAsync(buffer, CancellationToken.None);

        using JsonDocument qqUserInfoDocument = JsonDocument.Parse(buffer[..result.Count]);
        JsonElement qqUserInfo = qqUserInfoDocument.RootElement;

        JsonElement data = qqUserInfo.GetProperty("data");

        QqUserId = data.GetProperty("user_id").GetUInt32();
        string nickname = data.GetProperty("nickname").GetString() ?? string.Empty;

        Logger.Info($"QQ 连接成功：{nickname} ({QqUserId})");
    }

    private async Task ConnectDiscordAsync()
    {
        await Discord.LoginAsync(TokenType.Bot, Config.DiscordToken);
        await Discord.StartAsync();

        // Logger.Info($"Discord 登录成功：{Discord.CurrentUser.GlobalName} ({Discord.CurrentUser.Id})");
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            string cmdText = await File.ReadAllTextAsync("resources/initialize.sql");

            await using MySqlConnection connection = ConnectDatabase();
            await using MySqlCommand command = new MySqlCommand(cmdText, connection);

            await command.ExecuteNonQueryAsync();

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

    public Task ReconnectQqWebSocketAsync()
    {
        return ConnectQqWebSocketAsync();
    }

    public async Task StartAsync()
    {
        await InitializeDatabaseAsync();

        await Task.WhenAll(
            ConnectQqWebSocketAsync(),
            ConnectDiscordAsync());

        _eventTask = QqEvents.Initialize();
    }

    public Task WaitAsync()
    {
        return _eventTask ?? Task.CompletedTask;
    }
}