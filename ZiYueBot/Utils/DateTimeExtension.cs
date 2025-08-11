namespace ZiYueBot.Utils;

public static class DateTimeExtension
{
    public static DateTime ToYearMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }
}