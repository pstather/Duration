namespace Duration
{
  using System;

  public static class DateTimeExtensions
  {
    public static DateTime Add(this DateTime fromDate, Duration duration)
    {
      return fromDate + duration;
    }

    public static DateTimeOffset Add(this DateTimeOffset fromDate, Duration duration)
    {
      return fromDate + duration;
    }

    public static DateTime Subtract(this DateTime fromDate, Duration duration)
    {
      return fromDate - duration;
    }

    public static DateTimeOffset Subtract(this DateTimeOffset fromDate, Duration duration)
    {
      return fromDate - duration;
    }
  }
}