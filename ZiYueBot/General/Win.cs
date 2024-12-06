using log4net;
using MySql.Data.MySqlClient;
using ZiYueBot.Core;
using ZiYueBot.Utils;

namespace ZiYueBot.General;

public class Win : IGeneralCommand
{
    private static readonly ILog Logger = LogManager.GetLogger("赢");

    private struct WindWindow
    {
        public readonly DateTime Date = DateTime.Today;
        public readonly int WindHour = Random.Shared.Next(9, 21);
        public bool Blowed = false;

        public WindWindow()
        {
            Logger.Info($"今日风口：{WindHour} 时");
        }
    }

    private static readonly List<string> Levels = ["灵活赢。", "小赢。", "中赢。", "大赢。", "特大赢。", "赢麻了。", "输！"];

    private static readonly List<List<string>> Reviews =
    [
        [
            "我真的觉得我们千万不能太天真。", "好像真的要出大问题。", "现在这个水准还是太低了。",
            "我们决不允许这样。", "这个差距将被克服。", "真是什么问题都不能回避了。"
        ],
        [
            "我觉得我们真的要自信一点。", "只要你自信，怎么表达都可以。", "我们一点都不害怕竞争。",
            "我们的回旋余地特别大。", "很显然就是觉得不服气。"
        ],
        [
            "我想更精彩的故事还在后面。", "这使美国感到害怕了。", "现在确实在开始超越美国了。",
            "至少美国今天还做不到。"
        ],
        [
            "这个趋势还会持续下去。", "我们已经不是一般的先进了。", "我们不是一般的领先，对不对？",
            "别人都不可能超越我们。", "很好地展示了一种自信。", "这是基本的趋势。", "怎么评价都不过分。"
        ],
        [
            "这是中国崛起最精彩的地方。", "我们已经对美国形成了巨大的压力。", "必须给美国迎头痛击！",
            "你真可能会创造世界奇迹的。", "这种自信令人有点回味无穷。", "完胜所有西方国家。",
            "孰优孰劣一目了然。"
        ],
        [
            "已经震撼了这个世界。", "这是一种发自内心的钦佩。", "这种震撼效果前所未有。", "至今引以为荣。",
            "结果是一锤定音、釜底抽薪的胜利。"
        ],
        [
            "你赢赢赢，最后是输光光。"
        ],
        [
            "现在美国竞争不过我们。", "我们要更上一层楼了。", "我们手中的牌太多了。",
            "现在我们有很多新的牌可以打。", "该出手的时候一定要出手。", "局面马上就打开了。", "通过了这场全方位的压力测试。"
        ],
        [
            "令人感动之至。", "有时候是能合作共赢的。", "不要再不自信了。", "这一定是美丽的。"
        ]
    ];

    private static WindWindow _windWindow = new WindWindow();

    private int GetWinLevel(int score)
    {
        return score switch // 这是人类能理解的吗
        {
            >= 100 => 6,
            >= 98 => 5,
            >= 93 => 4,
            >= 76 => 3,
            >= 51 => 2,
            >= 3 => 1,
            _ => 0
        };
    }

    private string GetReview(int level)
    {
        return Reviews[level][Random.Shared.Next(0, Reviews[level].Count - 1)] + (level == 0 ? "\n（提示：试试再win一次）" : "");
    }

    /// <summary>
    /// 获取今日风口（时）
    /// </summary>
    private static int GetWindWindowHour()
    {
        return _windWindow.Date == DateTime.Today
            ? _windWindow.WindHour
            : (_windWindow = new WindWindow()).WindHour;
    }

    public string GetCommandId()
    {
        return "win";
    }

    public string GetCommandName()
    {
        return "赢";
    }

    public string GetCommandDescription()
    {
        return """
               /win
               以张维为教授为主题的“今日人品”命令。
               本命令包括的事件有：共同富 win、精准扶win、风口飞 win，以及心心相 win。详细信息请查看在线文档。
               在线文档：https://docs.ziyuebot.cn/win.html
               """;
    }

    public string GetCommandShortDescription()
    {
        return "我们又赢了！";
    }

    public Platform GetSupportedPlatform()
    {
        return Platform.Both;
    }

    private string Invoke(string userName, ulong userId, string channel) // 这里 channel 不再转换成 ulong，因为跟数据库交互不需要转换
    {
        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand query = new MySqlCommand(
            $"SELECT * FROM win WHERE userid = {userId} AND channel = {channel} LIMIT 1",
            database);
        using MySqlDataReader reader = query.ExecuteReader();
        bool hasRecord;
        bool targetedPovertyAlleviation = false;
        if (hasRecord = reader.Read())
        {
            DateTime queryDate = reader.GetDateTime("date");
            int score = reader.GetInt16("score");
            targetedPovertyAlleviation = reader.GetInt16("miniWinDays") >= 3;
            if (queryDate == DateTime.Today && score > 0)
            {
                return $"""
                        {userName} 已经在 {queryDate:MM 月 dd 日}赢过了，请明天再继续赢。
                        你今天的赢级是：{score}%，属于{Levels[GetWinLevel(score)]}
                        """;
            }
        }

        reader.Close();
        int rate = Random.Shared.Next(0, 100);
        bool blowed = DateTime.Now.Hour == GetWindWindowHour() && (_windWindow.Blowed = false);
        if (blowed)
        {
            rate = (int)Math.Ceiling(rate * 1.4);
            _windWindow.Blowed = true;
        }

        using MySqlCommand insert = new MySqlCommand(
            hasRecord
                ? $"UPDATE win SET date = current_date(), username = '{userName}', score = {rate}, prospered = false WHERE userid = {userId} AND channel = {channel}"
                : $"INSERT INTO win VALUE({userId}, '{userName}', {channel}, current_date(), {rate}, false, 0)",
            database);
        insert.ExecuteNonQuery();
        int level = GetWinLevel(rate);
        if (level == 1)
        {
            using MySqlCommand update = new MySqlCommand(
                $"UPDATE win SET miniWinDays = miniWinDays + 1, prospered = false WHERE userid = {userId} AND channel = {channel}",
                database
            );
            update.ExecuteNonQuery();
        }

        if (blowed)
        {
            return $"""
                    恭喜 {userName} 在 {DateTime.Today:MM 月 dd 日}乘上风口，赢级提高 40%！
                    {userName} 的赢级是：{rate}%，属于{Levels[level]}
                    维为寄语：{GetReview(level)}
                    """;
        }

        if (targetedPovertyAlleviation)
        {
            using MySqlCommand update = new MySqlCommand(
                $"UPDATE win SET miniWinDays = 0, prospered = true WHERE userid = {userId} AND channel = {channel}",
                database
            );
            update.ExecuteNonQuery();
            return $"""
                    恭喜 {userName} 在 {DateTime.Today:MM 月 dd 日}受到精准扶 win，赢级提高 40%！
                    {userName} 的赢级是：{rate}%，属于{Levels[level]}
                    维为寄语：{GetReview(7)}
                    """;
        }

        return $"""
                恭喜 {userName} 在 {DateTime.Today:MM 月 dd 日}赢了一次！
                {userName} 的赢级是：{rate}%，属于{Levels[level]}
                维为寄语：{GetReview(level)}
                """;
    }

    public bool TryCommonProsperity(ulong userId, string userName, string channel, out string message)
    {
        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand score = new MySqlCommand(
            $"SELECT * FROM win WHERE userid = {userId} AND channel = {channel} LIMIT 1",
            database
        );
        using MySqlDataReader scoreReader = score.ExecuteReader();
        if (scoreReader.Read() && !scoreReader.GetBoolean("prospered"))
        {
            int oldRate = scoreReader.GetInt16("score");
            if (GetWinLevel(oldRate) <= 2)
            {
                scoreReader.Close();
                using MySqlCommand query = new MySqlCommand(
                    $"SELECT * FROM win WHERE userid != {userId} AND channel = {channel} ORDER BY score DESC LIMIT 1",
                    database
                );
                using MySqlDataReader queryReader = query.ExecuteReader();
                if (queryReader.Read())
                {
                    if (GetWinLevel(queryReader.GetInt16("score")) >= 3)
                    {
                        int rate = (int)Math.Ceiling((double)(oldRate + queryReader.GetInt16("score")) / 2);
                        string user = queryReader.GetString("username");
                        queryReader.Close();
                        using MySqlCommand update = new MySqlCommand(
                            $"UPDATE win SET score = {rate}, miniWinDays = 0, prospered = true WHERE userid = {userId} AND channel = {channel}",
                            database
                        );
                        update.ExecuteNonQuery();
                        message = $"""
                                   恭喜 {userName} 在 {user} 的帮扶下实现共同富 win，使赢级达到了 {rate}%！
                                   维为寄语：{GetReview(8)}
                                   """;
                        return true;
                    }

                    message = "最赢者不够努力，赢级尚未达到大赢，无力帮扶。";
                    return true;
                }
            }
        }

        message = "";
        return false;
    }

    public bool SeekWinningCouple(ulong userId, string userName, string channel, out string message)
    {
        using MySqlConnection database = ZiYueBot.Instance.ConnectDatabase();
        using MySqlCommand query = new MySqlCommand(
            $"""
             SELECT score INTO @rate FROM win WHERE userid = {userId} AND channel = {channel} LIMIT 1; # 断言查询到的一定是今天的
             SELECT * FROM win WHERE userid != {userId} AND channel = {channel} AND score + @rate = 99 AND date = current_date() LIMIT 1;
             """, database
        );
        using MySqlDataReader reader = query.ExecuteReader();
        if (reader.Read())
        {
            message = $"""
                       恭喜 {userName} 与 {reader.GetString("username")} 的赢级之和达到 99，实现心心相 win！
                       愿你们永结同心，在未来的日子里风雨同舟、携手共赢！
                       """;
            return true;
        }

        message = "";
        return false;
    }

    public string QQInvoke(EventType eventType, string userName, uint userId, string[] args)
    {
        if (eventType == EventType.DirectMessage) return "独赢赢不如众赢赢，请在群组内使用该指令。";

        Logger.Info($"调用者：{userName} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");

        return Invoke(userName, userId, args[0]);
    }

    public string DiscordInvoke(EventType eventType, string userPing, ulong userId, string[] args)
    {
        if (eventType == EventType.DirectMessage) return "独赢赢不如众赢赢，请在群组内使用该指令。";

        Logger.Info($"调用者：{Message.MentionedUinAndName[userId]} ({userId})，参数：{MessageUtils.FlattenArguments(args)}");

        return Invoke(userPing, userId, args[0]);
    }
}