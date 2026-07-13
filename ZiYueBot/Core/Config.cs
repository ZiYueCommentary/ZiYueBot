namespace ZiYueBot.Core;

/// <summary>
/// Discord 及 MySQL 的相关配置。
/// 子悦机器初始化时会从根目录的 config.json 读取这些敏感信息。请勿公开 config.json。
/// 至于为什么必须是 MySQL，因为我服务器里有 MySQL。
/// </summary>
[Serializable]
public struct Config
{
    public string DiscordProxy { get; set; }
    public string DiscordToken { get; set; }
    public string DatabaseSource { get; set; }
    public int DatabasePort { get; set; }
    public string DatabaseName { get; set; }
    public string DatabaseUser { get; set; }
    public string DatabasePassword { get; set; }
    public string ChatAgentEndpoint { get; set; }
    public string BailianApiEndpoint { get; set; }
    public string BailianApiKey { get; set; }
    public string QqEventEndpoint { get; set; }
    public string QqEventAuthenticate { get; set; }
    public string QqApiEndpoint { get; set; }
    public string QqApiAuthenticate { get; set; }
    public string AssetsEndpoint { get; set; }
    public string AssetsUploadRegion { get; set; }
    public string AssetsUploadBucket { get; set; }
    public string AssetsUploadSecretId { get; set; }
    public string AssetsUploadSecretKey { get; set; }
}