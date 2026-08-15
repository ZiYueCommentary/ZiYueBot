namespace ZiYueBot.Core;

/// <summary>
/// Discord 及 MySQL 的相关配置。
/// 子悦机器初始化时会从根目录的 config.json 读取这些敏感信息。请勿公开 config.json。
/// 至于为什么必须是 MySQL，因为我服务器里有 MySQL。
/// </summary>
[Serializable]
public struct Config
{
}