using SkiaSharp;
using ZiYueBot.Core;

namespace ZiYueBot.Utils;

public static class StringExtension
{
    public static string DatabaseFriendly(this string str)
    {
        string result = "";
        bool simpleMessage = true;
        int pos = 0;
        for (int i = 0; i < str.Length; i++)
        {
            switch (str[i])
            {
                case '\u2402': // 图片
                {
                    result += str.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                    int end = str.IndexOf('\u2403', i + 1);
                    byte[] fileData = WebUtils.DownloadFile(str.Substring(i + 1, end - i - 1));
                    using SKData? data = SKData.CreateCopy(fileData);
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

                    Directory.CreateDirectory($"data/images/{DateTime.Today.Date:yyyy-MM-dd}");
                    string path = $"data/images/{DateTime.Today.Date:yyyy-MM-dd}/{Guid.NewGuid()}.{type}";
                    File.WriteAllBytesAsync(path, fileData);
                    result += $"\u2408{path}\u2409";
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
                case '\u2404': // 提及
                {
                    result += str.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                    int end = str.IndexOf('\u2405', i + 1);
                    result += $" @{Message.MentionedUinAndName[ulong.Parse(str.Substring(i + 1, end - i - 1))]} ";
                    if (i == 0) result = result[1..];
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
                case '<': // Discord 提及
                {
                    result += str.Substring(pos, i - pos - (pos == 0 ? 0 : 1));
                    if (str.IndexOf('@', i + 1) != i + 1)
                    {
                        continue;
                    }

                    int end = str.IndexOf('>', i + 1);
                    result += $" @{Message.MentionedUinAndName[ulong.Parse(str.Substring(i + 2, end - i - 2))]} ";
                    if (i == 0) result = result[1..];
                    i = pos = end;
                    simpleMessage = false;
                    continue;
                }
            }
        }

        if (simpleMessage) return str;

        if (pos < str.Length - 1) result += str[(pos + (str[pos + 1] == ' ' ? 2 : 1))..];
        return result;
    }

    public static string JsonFriendly(this string str)
    {
        return str.Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    public static string SafeArgument(this string str)
    {
        return str.Replace('\u2402', '[').Replace('\u2403', ']')
            .Replace('\u2404', '[').Replace('\u2405', ']')
            .Replace('\u2406', '[').Replace('\u2407', ']')
            .Replace('\u2408', '[').Replace('\u2409', ']');
    }
}