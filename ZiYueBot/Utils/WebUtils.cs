using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using log4net;
using SkiaSharp;
using ZiYueBot.Core;

namespace ZiYueBot.Utils;

/// <summary>
/// 网络相关，包括下载和 S3 上传。
/// </summary>
public static class WebUtils
{
    private static readonly ILog Logger = LogManager.GetLogger("网络");
    public static readonly HttpClient Client = new HttpClient();
    private static readonly CosXml Cos = new CosXmlServer(
        new CosXmlConfig.Builder().SetRegion(ZiYueBot.Instance.Config.AssetsUploadRegion).Build(),
        new DefaultQCloudCredentialProvider(ZiYueBot.Instance.Config.AssetsUploadSecretId,
            ZiYueBot.Instance.Config.AssetsUploadSecretKey, 600));

    /// <summary>
    /// 下载指定文件。
    /// </summary>
    public static async Task DownloadFile(string url, string dst)
    {
        using HttpResponseMessage response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        await using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
        await using Stream streamToWriteTo = File.Open(dst, FileMode.Create);
        await streamToReadFrom.CopyToAsync(streamToWriteTo);
    }

    /// <summary>
    /// 下载指定文件并直接返回。
    /// </summary>
    public static async Task<byte[]> DownloadFile(string url)
    {
        return await Client.GetByteArrayAsync(url);
    }

    /// <summary>
    /// 获取 Discord 用于发送图片的 Stream。
    /// </summary>
    public static async Task<Stream> GetStreamAsync(this ImageMessageEntity image)
    {
        if (image.Path.StartsWith("base64://"))
        {
            string base64Data = image.Path["base64://".Length..];
            byte[] buffer = Convert.FromBase64String(base64Data);
            return new MemoryStream(buffer);
        }

        Uri uri = new Uri(image.Path);
        return uri.Scheme.ToLower() switch
        {
            "file" => File.OpenRead(uri.LocalPath),
            "http" or "https" => await Client.GetStreamAsync(uri),
            _ => throw new NotSupportedException($"Scheme '{uri.Scheme}' is not supported")
        };
    }

    public static string UploadToS3(ImageMessageEntity image)
    {
        using Stream stream = image.GetStreamAsync().GetAwaiter().GetResult();
        using SKData? data = SKData.Create(stream);
        using SKCodec? codec = SKCodec.Create(data);
        string type = codec.EncodedFormat switch
        {
            SKEncodedImageFormat.Bmp => "bmp",
            SKEncodedImageFormat.Gif => "gif",
            SKEncodedImageFormat.Ico => "ico",
            SKEncodedImageFormat.Jpeg => "jpg",
            SKEncodedImageFormat.Png => "png",
            SKEncodedImageFormat.Wbmp => "wbmp",
            SKEncodedImageFormat.Webp => "webp",
            SKEncodedImageFormat.Pkm => "pkm",
            SKEncodedImageFormat.Ktx => "ktx",
            SKEncodedImageFormat.Astc => "astc",
            SKEncodedImageFormat.Dng => "dng",
            SKEncodedImageFormat.Heif => "heif",
            SKEncodedImageFormat.Avif => "avif",
            SKEncodedImageFormat.Jpegxl => "jpegxl",
            _ => "bin"
        };

        string key = $"images/{DateTime.Today:yyyy-MM}/{Guid.NewGuid()}.{type}";

        PutObjectRequest request = new PutObjectRequest(ZiYueBot.Instance.Config.AssetsUploadBucket, key, data.AsStream());
        Cos.PutObject(request);
        return $"{ZiYueBot.Instance.Config.AssetsEndpoint}/{key}";
    }
}