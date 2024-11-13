namespace ZiYueBot.Discord;

/// <summary>
/// Discord 及 MySQL 的相关配置。
/// 子悦机器初始化时会从根目录的config.json读取这些敏感信息。请勿公开config.json。
/// 至于为什么必须是 MySQL，因为我服务器里有 MySQL。
/// </summary>
[Serializable]
public struct Config
{
    public string DiscordProxy { get; set; }
    public string DiscordToken { get; set; }
    public string ProxyUsername { get; set; }
    public string ProxyPassword { get; set; }
    public string DatabaseSource { get; set; }
    public int DatabasePort { get; set; }
    public string DatabaseName { get; set; }
    public string DatabaseUser { get; set; }
    public string DatabasePassword { get; set; }
}