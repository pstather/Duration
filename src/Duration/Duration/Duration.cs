namespace Duration;

using System.Text.RegularExpressions;

public struct Duration
{
  internal readonly int? Years;
  internal readonly int? Months;
  internal readonly int? Days;
  internal readonly int? Weeks;
  internal readonly double? Hours;
  internal readonly double? Minutes;
  internal readonly double? Seconds;

  private static readonly Regex Duration1Regex = new Regex(
    @"^P(?=.)((?'years'\d+)Y)?((?'months'\d+)M)?((?'days'\d+)D)?(T(?=.)((?'hours'\d+(\.\d+)?)H)?((?'minutes'\d+(\.\d+)?)M)?((?'seconds'\d+(\.\d+)?)S)?)?$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

  private static readonly Regex Duration2Regex = new Regex(
    @"^P(?=.)((?'weeks'\d+)W)?((?'days'\d+)D)?(T(?=.)((?'hours'\d+(\.\d+)?)H)?((?'minutes'\d+(\.\d+)?)M)?((?'seconds'\d+(\.\d+)?)S)?)?$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

  private Duration(int? years = null, int? months = null, int? days = null,
    double? hours = null, double? minutes = null, double? seconds = null)
  {
    Years = years;
    Months = months;
    Days = days;
    Hours = hours;
    Minutes = minutes;
    Seconds = seconds;
  }

  private Duration(int? weeks = null, int? days = null,
    double? hours = null, double? minutes = null, double? seconds = null)
  {
    Weeks = weeks;
    Days = days;
    Hours = hours;
    Minutes = minutes;
    Seconds = seconds;
  }

  public TimeSpan GetTimespan(DateTime? fromDate = null)
  {
    fromDate ??= DateTime.Now;
    var toDate = fromDate.Value + this;
    return toDate - fromDate.Value;
  }

  public override string ToString()
  {
    Dictionary<string, string> periods = [];
    Dictionary<string, string> times = [];

    if (Weeks is > 0)
    {
      periods.Add("W", Weeks.Value.ToString());
    }
    else
    {
      if (Years is > 0)
      {
        periods.Add("Y", Years.Value.ToString());
      }

      if (Months is > 0)
      {
        periods.Add("M", Months.Value.ToString());
      }
    }

    if (Days is > 0)
    {
      periods.Add("D", Days.Value.ToString());
    }

    if (Hours is > 0)
    {
      times.Add("H", Hours.Value.ToString("#.##"));
    }

    if (Minutes is > 0)
    {
      times.Add("M", Minutes.Value.ToString("#.##"));
    }

    if (Seconds is > 0)
    {
      times.Add("S", Seconds.Value.ToString("#.###"));
    }

    if (periods is not { Count: > 0 } && times is not { Count: > 0 })
    {
      return string.Empty;
    }

    var periodFormat = periods is { Count: > 0 }
      ? string.Join(string.Empty, periods.Select(x => $"{x.Value}{x.Key}"))
      : string.Empty;

    var timeFormat = times is { Count: > 0 }
      ? "T" + string.Join(string.Empty, times.Select(x => $"{x.Value}{x.Key}"))
      : string.Empty;

    return $"P{periodFormat}{timeFormat}";
  }

  public static Duration FromString(string duration)
  {
    int? years = null;
    int? months = null;
    int? weeks = null;
    int? days = null;
    double? hours = null;
    double? minutes = null;
    double? seconds = null;
    var duration1Match = Duration1Regex.Match(duration);
    if (duration1Match.Success)
    {
      if (duration1Match.Groups["years"].Success)
      {
        years = int.Parse(duration1Match.Groups["years"].Value);
      }

      if (duration1Match.Groups["months"].Success)
      {
        months = int.Parse(duration1Match.Groups["months"].Value);
      }

      if (duration1Match.Groups["days"].Success)
      {
        days = int.Parse(duration1Match.Groups["days"].Value);
      }

      if (duration1Match.Groups["hours"].Success)
      {
        hours = double.Parse(duration1Match.Groups["hours"].Value);
      }

      if (duration1Match.Groups["minutes"].Success)
      {
        minutes = double.Parse(duration1Match.Groups["minutes"].Value);
      }

      if (duration1Match.Groups["seconds"].Success)
      {
        seconds = double.Parse(duration1Match.Groups["seconds"].Value);
      }

      return new Duration(years, months, days, hours, minutes, seconds);
    }

    var duration2Match = Duration2Regex.Match(duration);
    if (duration2Match.Success)
    {
      if (duration2Match.Groups["weeks"].Success)
      {
        weeks = int.Parse(duration2Match.Groups["weeks"].Value);
      }

      if (duration2Match.Groups["days"].Success)
      {
        days = int.Parse(duration2Match.Groups["days"].Value);
      }

      if (duration2Match.Groups["hours"].Success)
      {
        hours = double.Parse(duration2Match.Groups["hours"].Value);
      }

      if (duration2Match.Groups["minutes"].Success)
      {
        minutes = double.Parse(duration2Match.Groups["minutes"].Value);
      }

      if (duration2Match.Groups["seconds"].Success)
      {
        seconds = double.Parse(duration2Match.Groups["seconds"].Value);
      }

      return new Duration(weeks, days, hours, minutes, seconds);
    }

    throw new InvalidDurationException(duration);
  }

  public static DateTime operator +(DateTime fromDate, Duration duration)
  {
    if (duration is { Weeks: > 0 })
    {
      fromDate = fromDate.AddDays(duration.Weeks.Value * 7);
    }
    else
    {
      fromDate = fromDate.AddYears(duration.Years ?? 0);
      fromDate = fromDate.AddMonths(duration.Months ?? 0);
    }

    fromDate = fromDate.AddDays(duration.Days ?? 0);
    fromDate = fromDate.AddHours(duration.Hours ?? 0);
    fromDate = fromDate.AddMinutes(duration.Minutes ?? 0);
    fromDate = fromDate.AddSeconds(duration.Seconds ?? 0);
    return fromDate;
  }

  public static DateTimeOffset operator +(DateTimeOffset fromDate, Duration duration)
  {
    if (duration is { Weeks: > 0 })
    {
      fromDate = fromDate.AddDays(duration.Weeks.Value * 7);
    }
    else
    {
      fromDate = fromDate.AddYears(duration.Years ?? 0);
      fromDate = fromDate.AddMonths(duration.Months ?? 0);
    }

    fromDate = fromDate.AddDays(duration.Days ?? 0);
    fromDate = fromDate.AddHours(duration.Hours ?? 0);
    fromDate = fromDate.AddMinutes(duration.Minutes ?? 0);
    fromDate = fromDate.AddSeconds(duration.Seconds ?? 0);
    return fromDate;
  }

  public static DateTime operator -(DateTime fromDate, Duration duration)
  {
    if (duration is { Weeks: > 0 })
    {
      fromDate = fromDate.AddDays(duration.Weeks.Value * 7 * -1);
    }
    else
    {
      fromDate = fromDate.AddYears((duration.Years ?? 0) * -1);
      fromDate = fromDate.AddMonths((duration.Months ?? 0) * -1);
    }

    fromDate = fromDate.AddDays((duration.Days ?? 0) * -1);
    fromDate = fromDate.AddHours((duration.Hours ?? 0) * -1);
    fromDate = fromDate.AddMinutes((duration.Minutes ?? 0) * -1);
    fromDate = fromDate.AddSeconds((duration.Seconds ?? 0) * -1);
    return fromDate;
  }

  public static DateTimeOffset operator -(DateTimeOffset fromDate, Duration duration)
  {
    if (duration is { Weeks: > 0 })
    {
      fromDate = fromDate.AddDays(duration.Weeks.Value * 7 * -1);
    }
    else
    {
      fromDate = fromDate.AddYears((duration.Years ?? 0) * -1);
      fromDate = fromDate.AddMonths((duration.Months ?? 0) * -1);
    }

    fromDate = fromDate.AddDays((duration.Days ?? 0) * -1);
    fromDate = fromDate.AddHours((duration.Hours ?? 0) * -1);
    fromDate = fromDate.AddMinutes((duration.Minutes ?? 0) * -1);
    fromDate = fromDate.AddSeconds((duration.Seconds ?? 0) * -1);
    return fromDate;
  }
}