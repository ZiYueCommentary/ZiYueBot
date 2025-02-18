using Discord;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using ZiYueBot.Discord;
using ZiYueBot.QQ;
using log4net;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using Discord.Net.Rest;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;

namespace ZiYueBot;

public class ZiYueBot
{
    public const string Version = "0.1.0";

    private static readonly ILog Logger = LogManager.GetLogger("主程序");
    public static ZiYueBot Instance;

    public readonly ClientWebSocket QqEvent;
    public readonly ClientWebSocket QqApi;
    public readonly DiscordSocketClient Discord;

    public readonly Config Config;

    private ZiYueBot()
    {
        QqEvent = new ClientWebSocket();
        QqEvent.ConnectAsync(new Uri("ws://127.0.0.1:3001/event/"), CancellationToken.None).Wait();
        QqApi = new ClientWebSocket();
        QqApi.ConnectAsync(new Uri("ws://127.0.0.1:3001/api/"), CancellationToken.None).Wait();
        Logger.Info("QQ - 连接成功！");

        using (FileStream stream = new FileStream("config.json", FileMode.OpenOrCreate, FileAccess.Read))
        {
            Config = JsonSerializer.Deserialize<Config>(stream);
        }

        Discord = new DiscordSocketClient(new DiscordSocketConfig
        {
            RestClientProvider = DefaultRestClientProvider.Create(true),
            WebSocketProvider = DefaultWebSocketProvider.Create(new WebProxy("http://127.0.0.1:7890"))
        });
        Discord.LoginAsync(TokenType.Bot, Config.DiscordToken).Wait();
        Discord.StartAsync().Wait();
        Logger.Info("Discord - 登录成功！");

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
                                                    CREATE TABLE win
                                                    (
                                                        id          int auto_increment primary key,
                                                        userid      bigint                    null,
                                                        username    tinytext                  null,
                                                        channel     bigint                    null,
                                                        date        date                      null,
                                                        score       tinyint                   null,
                                                        prospered   boolean          default false,
                                                        miniWinDays tinyint              default 0
                                                    ) CHARSET = utf8mb4;
                                                    """, database);
            command.ExecuteNonQuery();
        }
        catch (MySqlException)
        {
        }
    }

    public MySqlConnection ConnectDatabase()
    {
        MySqlConnection connection = new MySqlConnection(
            $"""
             server={Config.DatabaseSource};
             port={Config.DatabasePort};
             database={Config.DatabaseName};
             user={Config.DatabaseUser};
             password={Config.DatabasePassword};
             charset=utf8mb4;
             allowuservariables=True;
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
        Handler.Initialize();
        Events.Initialize().Wait();
    }
}