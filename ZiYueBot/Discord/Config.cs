namespace ZiYueBot.Discord;

/// <summary>
/// Discord 机器人的相关配置。包括代理地址、代理用户名、代理密码，以及机器人Token。
/// 子悦机器初始化时会从根目录的config.json读取这些敏感信息。请勿公开config.json。
/// </summary>
[Serializable]
public struct Config
{
    public string Proxy { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Token { get; set; }
}