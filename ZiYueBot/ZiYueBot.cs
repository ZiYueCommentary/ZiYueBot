using CCB.Abstractions;
using Microsoft.Data.Sqlite;
using Serilog;
using ZiYueBot.Core;

namespace ZiYueBot;

public class ZiYueBot : IPluginMetadata
{
    // private static readonly ILog Logger = LogManager.GetLogger("主程序");
    public static ZiYueBot Instance { get; private set; } = null!;

    private Task? _eventTask;

    public static ZiYueBot Create()
    {
        Instance = new ZiYueBot();

        Commands.Initialize();

        return Instance;
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            string cmdText = await File.ReadAllTextAsync("resources/initialize.sql");

            await using SqliteConnection connection = ConnectDatabase();
            await using SqliteCommand command = new SqliteCommand(cmdText, connection);

            command.ExecuteNonQuery();

            Log.Information("数据库初始化成功");
        }
        catch (Exception e)
        {
            Log.Error("数据库初始化出错 " + e.StackTrace);
        }
    }

    public SqliteConnection ConnectDatabase()
    {
        SqliteConnection connection = new SqliteConnection("Data Source=ziyuebot.db;");
        connection.Open();
        return connection;
    }

    public async Task StartAsync()
    {
        await InitializeDatabaseAsync();

        await Task.WhenAll();
    }

    public Task WaitAsync()
    {
        return _eventTask ?? Task.CompletedTask;
    }

    public string Name => "子悦机器";
    public string Description => "轻量高级子悦机器";
    public string Author => "ZiYue";
}