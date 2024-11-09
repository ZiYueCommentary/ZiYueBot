namespace ZiYueBot.Utils;

public record Image(string Url)
{
    public override string ToString()
    {
        return $"\u2402{Url}\u2403";
    }
}