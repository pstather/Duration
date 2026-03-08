namespace Duration;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public readonly struct Duration : IEquatable<Duration>
{
  private readonly int? _years;
  private readonly int? _months;
  private readonly int? _days;
  private readonly int? _weeks;
  private readonly double? _hours;
  private readonly double? _minutes;
  private readonly double? _seconds;

  private static readonly Regex[] RegexArray =
  [
    new Regex(
      @"^P(?=.)((?'years'\d+)Y)?((?'months'\d+)M)?((?'days'\d+)D)?(T(?=.)((?'hours'\d+)H)?((?'minutes'\d+)M)?((?'seconds'\d+((\.)\d+)?)S)?)?$",
      RegexOptions.IgnoreCase | RegexOptions.Compiled),
    new Regex(
      @"^P(?=.)((?'years'\d+)Y)?((?'months'\d+)M)?((?'days'\d+)D)?(T(?=.)((?'hours'\d+)H)?((?'minutes'\d+((\.)\d+)?)M)?)?$",
      RegexOptions.IgnoreCase | RegexOptions.Compiled),
    new Regex(
      @"^P(?=.)((?'years'\d+)Y)?((?'months'\d+)M)?((?'days'\d+)D)?(T(?=.)((?'hours'\d+((\.)\d+)?)H)?)?$",
      RegexOptions.IgnoreCase | RegexOptions.Compiled),
    new Regex(
      @"^P(?=.)((?'weeks'\d+)W)$",
      RegexOptions.IgnoreCase | RegexOptions.Compiled)
  ];

  private Duration(int? years = null, int? months = null, int? weeks = null, int? days = null,
    double? hours = null, double? minutes = null, double? seconds = null)
  {
    _years = years;
    _months = months;
    _weeks = weeks;
    _days = days;
    _hours = hours;
    _minutes = minutes;
    _seconds = seconds;
  }

  public TimeSpan GetTimespan(DateTime? fromDate = null)
  {
    fromDate ??= DateTime.Now;
    var toDate = fromDate.Value + this;
    return toDate - fromDate.Value;
  }

  public override string ToString()
  {
    if (this is
        {
          _years: not > 0, _months: not > 0, _weeks: not > 0, _days: not > 0, _hours: not > 0, _minutes: not > 0,
          _seconds: not > 0
        })
    {
      return "PT0S";
    }

    var builder = new StringBuilder();
    builder.Append('P');
    if (_weeks is > 0)
    {
      builder.Append(_weeks.Value).Append('W');
      return builder.ToString();
    }

    if (_years is > 0)
    {
      builder.Append(_years.Value).Append('Y');
    }

    if (_months is > 0)
    {
      builder.Append(_months.Value).Append('M');
    }

    if (_days is > 0)
    {
      builder.Append(_days.Value).Append('D');
    }

    if (this is { _hours: not > 0, _minutes: not > 0, _seconds: not > 0 })
    {
      return builder.ToString();
    }

    builder.Append('T');
    if (_hours is > 0)
    {
      builder.Append(_hours.Value.ToString("0.###", CultureInfo.InvariantCulture)).Append('H');
    }

    if (_minutes is > 0)
    {
      builder.Append(_minutes.Value.ToString("0.###", CultureInfo.InvariantCulture)).Append('M');
    }

    if (_seconds is > 0)
    {
      builder.Append(_seconds.Value.ToString("0.###", CultureInfo.InvariantCulture)).Append('S');
    }

    return builder.ToString();
  }

  public override bool Equals([NotNullWhen(true)] object? obj)
  {
    return obj is Duration duration  && Equals(duration);
  }

  public bool Equals(Duration other)
  {
    return Nullable.Equals(_years, other._years)
           && Nullable.Equals(_months, other._months)
           && Nullable.Equals(_weeks, other._weeks)
           && Nullable.Equals(_days, other._days)
           && Nullable.Equals(_hours, other._hours)
           && Nullable.Equals(_minutes, other._minutes)
           && Nullable.Equals(_seconds, other._seconds);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(_years, _months, _days, _weeks, _hours, _minutes, _seconds);
  }

  public static readonly Duration Zero = new();

  public static Duration FromString(string duration)
  {
    int? years = null;
    int? months = null;
    int? weeks = null;
    int? days = null;
    double? hours = null;
    double? minutes = null;
    double? seconds = null;

    var matched = false;
    foreach (var regex in RegexArray)
    {
      var match = regex.Match(duration);
      if (!match.Success)
      {
        continue;
      }

      matched = true;
      if (match.Groups["weeks"].Success)
      {
        weeks = int.Parse(match.Groups["weeks"].Value);
        break;
      }

      if (match.Groups["years"].Success)
      {
        years = int.Parse(match.Groups["years"].Value);
      }

      if (match.Groups["months"].Success)
      {
        months = int.Parse(match.Groups["months"].Value);
      }

      if (match.Groups["days"].Success)
      {
        days = int.Parse(match.Groups["days"].Value);
      }

      if (match.Groups["hours"].Success)
      {
        hours = double.Parse(match.Groups["hours"].Value, CultureInfo.InvariantCulture);
      }

      if (match.Groups["minutes"].Success)
      {
        minutes = double.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture);
      }

      if (match.Groups["seconds"].Success)
      {
        seconds = double.Parse(match.Groups["seconds"].Value, CultureInfo.InvariantCulture);
      }

      break;
    }

    return !matched
      ? throw new InvalidDurationException()
      : new Duration(years, months, weeks, days, hours, minutes, seconds);
  }

  public static Duration FromTimeSpan(TimeSpan timeSpan)
  {
    return new Duration(
      days: timeSpan.Days,
      hours: timeSpan.Hours,
      minutes: timeSpan.Minutes,
      seconds: timeSpan.Seconds + ((double)timeSpan.Milliseconds / 1000));
  }

  public static Duration AbsoluteDifference(DateTime startDate, DateTime endDate)
  {
    DateTime biggestDate;
    DateTime smallestDate;

    if (startDate == endDate)
    {
      return Zero;
    }

    if (startDate > endDate)
    {
      biggestDate = startDate;
      smallestDate = endDate;
    }
    else
    {
      biggestDate = endDate;
      smallestDate = startDate;
    }

    var years = biggestDate.Year - smallestDate.Year;
    if (years > 0)
    {
      if (smallestDate.AddYears(years) > biggestDate)
      {
        years -= 1;
      }
    }

    smallestDate = smallestDate.AddYears(years);
    if (smallestDate == biggestDate)
    {
      return new Duration(years: years);
    }

    int months = 0;
    for (var i = 1; i <= 12; i++)
    {
      months = i;
      if (smallestDate.AddMonths(months) > biggestDate)
      {
        months -= 1;
        break;
      }
    }

    smallestDate = smallestDate.AddMonths(months);
    if (smallestDate == biggestDate)
    {
      return new Duration(years: years, months: months);
    }

    var timespan = biggestDate - smallestDate;
    return new Duration(
      years: years,
      months: months,
      days: timespan.Days,
      hours: timespan.Hours,
      minutes: timespan.Minutes,
      seconds: timespan.Seconds + ((double)timespan.Milliseconds / 1000));
  }

  public static bool operator ==(Duration duration1, Duration duration2)
  {
    return duration1.Equals(duration2);
  }

  public static bool operator !=(Duration duration1, Duration duration2)
  {
    return !duration1.Equals(duration2);
  }

  public static DateTime operator +(DateTime fromDate, Duration duration)
  {
    if (duration is { _weeks: > 0 })
    {
      fromDate = fromDate.AddDays(duration._weeks.Value * 7);
    }
    else
    {
      fromDate = fromDate.AddYears(duration._years ?? 0);
      fromDate = fromDate.AddMonths(duration._months ?? 0);
    }

    fromDate = fromDate.AddDays(duration._days ?? 0);
    fromDate = fromDate.AddHours(duration._hours ?? 0);
    fromDate = fromDate.AddMinutes(duration._minutes ?? 0);
    fromDate = fromDate.AddSeconds(duration._seconds ?? 0);
    return fromDate;
  }

  public static DateTimeOffset operator +(DateTimeOffset fromDate, Duration duration)
  {
    if (duration is { _weeks: > 0 })
    {
      fromDate = fromDate.AddDays(duration._weeks.Value * 7);
    }
    else
    {
      fromDate = fromDate.AddYears(duration._years ?? 0);
      fromDate = fromDate.AddMonths(duration._months ?? 0);
    }

    fromDate = fromDate.AddDays(duration._days ?? 0);
    fromDate = fromDate.AddHours(duration._hours ?? 0);
    fromDate = fromDate.AddMinutes(duration._minutes ?? 0);
    fromDate = fromDate.AddSeconds(duration._seconds ?? 0);
    return fromDate;
  }

  public static DateTime operator -(DateTime fromDate, Duration duration)
  {
    if (duration is { _weeks: > 0 })
    {
      fromDate = fromDate.AddDays(duration._weeks.Value * 7 * -1);
    }
    else
    {
      fromDate = fromDate.AddYears((duration._years ?? 0) * -1);
      fromDate = fromDate.AddMonths((duration._months ?? 0) * -1);
    }

    fromDate = fromDate.AddDays((duration._days ?? 0) * -1);
    fromDate = fromDate.AddHours((duration._hours ?? 0) * -1);
    fromDate = fromDate.AddMinutes((duration._minutes ?? 0) * -1);
    fromDate = fromDate.AddSeconds((duration._seconds ?? 0) * -1);
    return fromDate;
  }

  public static DateTimeOffset operator -(DateTimeOffset fromDate, Duration duration)
  {
    if (duration is { _weeks: > 0 })
    {
      fromDate = fromDate.AddDays(duration._weeks.Value * 7 * -1);
    }
    else
    {
      fromDate = fromDate.AddYears((duration._years ?? 0) * -1);
      fromDate = fromDate.AddMonths((duration._months ?? 0) * -1);
    }

    fromDate = fromDate.AddDays((duration._days ?? 0) * -1);
    fromDate = fromDate.AddHours((duration._hours ?? 0) * -1);
    fromDate = fromDate.AddMinutes((duration._minutes ?? 0) * -1);
    fromDate = fromDate.AddSeconds((duration._seconds ?? 0) * -1);
    return fromDate;
  }
}