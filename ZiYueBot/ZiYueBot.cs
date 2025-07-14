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
    public const string Version = "0.1.2";

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
                                                        userid                bigint      default 0,
                                                        username              tinytext         null,
                                                        channel               bigint      default 0,
                                                        date                  date             null,
                                                        score                 tinyint          null,
                                                        prospered             boolean default false,
                                                        miniWinDays           tinyint     default 0,
                                                        invoke_days           int         default 0,
                                                        flexible_win_days     int         default 0,
                                                        mini_win_days         int         default 0,
                                                        middle_win_days       int         default 0,
                                                        big_win_days          int         default 0,
                                                        very_big_win_days     int         default 0,
                                                        ultra_win_days        int         default 0,
                                                        lose_days             int         default 0,
                                                        couple_win_days       int         default 0,
                                                        wind_window_days      int         default 0,
                                                        alleviated_days       int         default 0,
                                                        prosperity_days       int         default 0,
                                                        prosperity_other_days int         default 0,
                                                        PRIMARY KEY (userid, channel)
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
                                                    CREATE TABLE aprilbottles
                                                    (
                                                        id       int auto_increment primary key,
                                                        username tinytext                  null,
                                                        created  date                      null,
                                                        content  text                      null
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
                                                    CREATE TABLE blacklists
                                                    (
                                                        userid  bigint          default 0,
                                                        command varchar(50) default 'all',
                                                        time    datetime             null,
                                                        reason  text                 null,
                                                        PRIMARY KEY (userid, command)
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
                                                    CREATE TABLE sponsors
                                                    (
                                                        userid  bigint default 0 primary key,
                                                        expiry  date                    null
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
                                                    CREATE TABLE invoke_records_general
                                                    (
                                                        userid       bigint      not null,
                                                        command      varchar(50) not null,
                                                        first_invoke datetime    null,
                                                        last_invoke  datetime    null,
                                                        invoke_count int         null,
                                                        PRIMARY KEY (userid, command)
                                                    ) CHARSET = utf8mb4;

                                                    CREATE INDEX invoke_records_general_index_userid
                                                    ON invoke_records_general (userid);
                                                    """, database);
            command.ExecuteNonQuery();
        }
        catch (MySqlException)
        {
        }
        
        try
        {
            MySqlCommand command = new MySqlCommand("""
                                                    CREATE TABLE revolver
                                                    (
                                                        userid               bigint   not null primary key,
                                                        first_invoke         datetime null,
                                                        last_invoke          datetime null,
                                                        start_count          int      default 0,
                                                        shooting_self_count  int      default 0,
                                                        shooting_other_count int      default 0,
                                                        shooting_self_death  int      default 0,
                                                        shooting_other_death int      default 0,
                                                        rotating_count       int      default 0,
                                                        restart_count        int      default 0
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
        Handler.Initialize();
        Events.Initialize().Wait();
    }
}