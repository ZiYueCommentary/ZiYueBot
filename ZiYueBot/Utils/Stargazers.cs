using System.Text.RegularExpressions;
using log4net;
using MySql.Data.MySqlClient;

namespace ZiYueBot.Utils;

public static partial class Stargazers
{
    private static readonly ILog Logger = LogManager.GetLogger("云瓶星标");

    public static int GetStargazerCount(int bottleId)
    {
        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand command =
            new MySqlCommand($"SELECT (SELECT COUNT(*) FROM stargazers WHERE bottle_id = {bottleId} AND removed = 0) AS count", database);
        using MySqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? reader.GetInt32("count") : 0;
    }

    public static string AddStargazer(ulong userId, string userName, int bottleId)
    {
        Logger.Info($"{userId} 星标了 {bottleId} 号云瓶");
        {
            using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
            using MySqlCommand command =
                new MySqlCommand($"SELECT * FROM stargazers WHERE userid = {userId} AND bottle_id = {bottleId}",
                    database);
            using MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                if (!reader.GetBoolean("removed")) return $"{userName}：您已星标过 {bottleId} 号云瓶";
            }
        }
        {
            using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
            using MySqlCommand command =
                new MySqlCommand(
                    $"""
                     INSERT INTO stargazers(userid, star_at, bottle_id) VALUE ({userId}, now(), {bottleId})
                     ON DUPLICATE KEY UPDATE star_at = now(), removed = 0
                     """,
                    database);
            command.ExecuteNonQuery();
            return $"{userName} 已星标 {bottleId} 号云瓶！对云瓶消息回应“点赞”图标即可星标~";
        }
    }

    [GeneratedRegex("你捞到了 (\\d+)* 号瓶子！")]
    public static partial Regex StargazerRegex();
}