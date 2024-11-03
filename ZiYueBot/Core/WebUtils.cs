namespace ZiYueBot.Core;

/// <summary>
/// 网络相关。
/// </summary>

public static class WebUtils
{
    /// <summary>
    /// 下载指定文件。
    /// </summary>
    public static async Task DownloadFile(string url, string destinationPath)
    {
        using HttpClient client = new HttpClient();
        using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        await using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
        await using Stream streamToWriteTo = File.Open(destinationPath, FileMode.Create);
        await streamToReadFrom.CopyToAsync(streamToWriteTo);
    }
}