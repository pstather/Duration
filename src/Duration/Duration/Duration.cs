namespace Duration
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Globalization;
  using System.Text;
  using System.Text.RegularExpressions;

  public readonly struct Duration : IEquatable<Duration>
  {
    private readonly bool _negative;
    private readonly int _years;
    private readonly int _months;
    private readonly int _weeks;
    private readonly double _days;
    private readonly double _hours;
    private readonly double _minutes;
    private readonly double _seconds;

    private static readonly Regex DurationRegex =
      new Regex(
        @"^(?'sign'[+-])?P(?:(?'weeks'\d+)W|(?=.)(?:(?'years'\d+)Y)?(?:(?'months'\d+)M)?(?:(?'days'\d+(?:\.\d+)?)D)?(?:T(?=.)(?:(?'hours'\d+(?:\.\d+)?)H)?(?:(?'minutes'\d+(?:\.\d+)?)M)?(?:(?'seconds'\d+(?:\.\d+)?)S)?)?)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private Duration(bool negative = false, int years = 0, int months = 0, int weeks = 0, double days = 0,
      double hours = 0, double minutes = 0, double seconds = 0)
    {
      _negative = negative;
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
      if (_years == 0
          && _months == 0
          && _weeks == 0
          && _days == 0
          && _hours == 0
          && _minutes == 0
          && _seconds == 0)
      {
        return "PT0S";
      }

      var builder = new StringBuilder();
      if (_negative)
      {
        builder.Append('-');
      }

      builder.Append('P');
      if (_weeks > 0)
      {
        builder.Append(_weeks).Append('W');
        return builder.ToString();
      }

      if (_years > 0)
      {
        builder.Append(_years).Append('Y');
      }

      if (_months > 0)
      {
        builder.Append(_months).Append('M');
      }

      if (_days > 0)
      {
        builder.Append(_days.ToString("0.##", CultureInfo.InvariantCulture)).Append('D');
      }

      if (_hours == 0
          && _minutes == 0
          && _seconds == 0)
      {
        return builder.ToString();
      }

      builder.Append('T');
      if (_hours > 0)
      {
        builder.Append(_hours.ToString("0.###", CultureInfo.InvariantCulture)).Append('H');
      }

      if (_minutes > 0)
      {
        builder.Append(_minutes.ToString("0.###", CultureInfo.InvariantCulture)).Append('M');
      }

      if (_seconds > 0)
      {
        builder.Append(_seconds.ToString("0.###", CultureInfo.InvariantCulture)).Append('S');
      }

      return builder.ToString();
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is Duration duration  && Equals(duration);
    }

    public bool Equals(Duration other)
    {
      return _negative.Equals(other._negative)
             && _years.Equals(other._years)
             && _months.Equals(other._months)
             && _weeks.Equals(other._weeks)
             && _days.Equals(other._days)
             && _hours.Equals(other._hours)
             && _minutes.Equals(other._minutes)
             && _seconds.Equals(other._seconds);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(_negative, _years, _months, _weeks, _days, _hours, _minutes, _seconds);
    }

    public static readonly Duration Zero = new Duration();

    public static Duration FromString(string duration)
    {
      var negative = false;
      int years = 0;
      int months = 0;
      int weeks = 0;
      double days = 0;
      double hours = 0;
      double minutes = 0;
      double seconds = 0;

        var match = DurationRegex.Match(duration);
        if (!match.Success)
        {
          throw new InvalidDurationException();
        }

        if (match.Groups["sign"].Success)
        {
          negative = match.Groups["sign"].Value == "-";
        }

        if (match.Groups["weeks"].Success)
        {
          weeks = int.Parse(match.Groups["weeks"].Value, CultureInfo.InvariantCulture);
          return new Duration(negative, years, months, weeks, days, hours, minutes, seconds);
        }

        if (match.Groups["years"].Success)
        {
          years = int.Parse(match.Groups["years"].Value, CultureInfo.InvariantCulture);
        }

        if (match.Groups["months"].Success)
        {
          months = int.Parse(match.Groups["months"].Value, CultureInfo.InvariantCulture);
        }

        if (match.Groups["days"].Success)
        {
          days = double.Parse(match.Groups["days"].Value, CultureInfo.InvariantCulture);
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

      return new Duration(negative, years, months, weeks, days, hours, minutes, seconds);
    }

    public static Duration FromTimeSpan(TimeSpan timeSpan)
    {
      return new Duration(
        negative: timeSpan < TimeSpan.Zero,
        days: timeSpan.Days,
        hours: timeSpan.Hours,
        minutes: timeSpan.Minutes,
        seconds: timeSpan.Seconds + ((double)timeSpan.Milliseconds / 1000));
    }


    public static Duration Difference(DateTime startDate, DateTime endDate, bool absolute = true)
    {
      DateTime biggestDate;
      DateTime smallestDate;

      if (startDate == endDate)
      {
        return Zero;
      }

      var negative = false;
      if (startDate > endDate)
      {
        if (!absolute)
        {
          negative = true;
        }

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
        negative: negative,
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
      var multiplier = duration._negative ? -1 : 1;
      if (duration._weeks > 0)
      {
        fromDate = fromDate.AddDays(duration._weeks * 7 * multiplier);
      }
      else
      {
        fromDate = fromDate.AddYears(duration._years * multiplier);
        fromDate = fromDate.AddMonths(duration._months * multiplier);
      }

      fromDate = fromDate.AddDays(duration._days * multiplier);
      fromDate = fromDate.AddHours(duration._hours * multiplier);
      fromDate = fromDate.AddMinutes(duration._minutes * multiplier);
      fromDate = fromDate.AddSeconds(duration._seconds * multiplier);
      return fromDate;
    }

    public static DateTimeOffset operator +(DateTimeOffset fromDate, Duration duration)
    {
      var multiplier = duration._negative ? -1 : 1;
      if (duration._weeks > 0)
      {
        fromDate = fromDate.AddDays(duration._weeks * 7 * multiplier);
      }
      else
      {
        fromDate = fromDate.AddYears(duration._years * multiplier);
        fromDate = fromDate.AddMonths(duration._months * multiplier);
      }

      fromDate = fromDate.AddDays(duration._days * multiplier);
      fromDate = fromDate.AddHours(duration._hours * multiplier);
      fromDate = fromDate.AddMinutes(duration._minutes * multiplier);
      fromDate = fromDate.AddSeconds(duration._seconds * multiplier);
      return fromDate;
    }

    public static DateTime operator -(DateTime fromDate, Duration duration)
    {
      var multiplier = duration._negative ? -1 : 1;
      if (duration._weeks > 0)
      {
        fromDate = fromDate.AddDays(duration._weeks * 7 * -1 * multiplier);
      }
      else
      {
        fromDate = fromDate.AddYears(duration._years * -1 * multiplier);
        fromDate = fromDate.AddMonths(duration._months * -1 * multiplier);
      }

      fromDate = fromDate.AddDays(duration._days * -1 * multiplier);
      fromDate = fromDate.AddHours(duration._hours * -1 * multiplier);
      fromDate = fromDate.AddMinutes(duration._minutes * -1 * multiplier);
      fromDate = fromDate.AddSeconds(duration._seconds * -1 * multiplier);
      return fromDate;
    }

    public static DateTimeOffset operator -(DateTimeOffset fromDate, Duration duration)
    {
      var multiplier = duration._negative ? -1 : 1;
      if (duration._weeks > 0)
      {
        fromDate = fromDate.AddDays(duration._weeks * 7 * -1 * multiplier);
      }
      else
      {
        fromDate = fromDate.AddYears(duration._years * -1 * multiplier);
        fromDate = fromDate.AddMonths(duration._months * -1 * multiplier);
      }

      fromDate = fromDate.AddDays(duration._days * -1 * multiplier);
      fromDate = fromDate.AddHours(duration._hours * -1 * multiplier);
      fromDate = fromDate.AddMinutes(duration._minutes * -1 * multiplier);
      fromDate = fromDate.AddSeconds(duration._seconds * -1 * multiplier);
      return fromDate;
    }
  }
}