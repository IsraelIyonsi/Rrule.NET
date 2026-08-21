using System.Globalization;

namespace Rrule.Internal;

/// <summary>
/// Turns an RFC 5545 <c>RRULE</c> string into a <see cref="RecurrenceRule"/>.
/// Sole responsibility: lexing and validating rule parts.
/// </summary>
internal static class RruleParser
{
    private static readonly IReadOnlyDictionary<string, Frequency> Frequencies =
        new Dictionary<string, Frequency>(StringComparer.Ordinal)
        {
            ["SECONDLY"] = Frequency.Secondly,
            ["MINUTELY"] = Frequency.Minutely,
            ["HOURLY"] = Frequency.Hourly,
            ["DAILY"] = Frequency.Daily,
            ["WEEKLY"] = Frequency.Weekly,
            ["MONTHLY"] = Frequency.Monthly,
            ["YEARLY"] = Frequency.Yearly,
        };

    public static RecurrenceRule Parse(string rrule)
    {
        if (rrule is null)
        {
            throw new RruleParseException("The RRULE string must not be null.");
        }

        var body = StripPrefix(rrule.Trim());
        if (body.Length == 0)
        {
            throw new RruleParseException("The RRULE string must not be empty.");
        }

        Frequency? frequency = null;
        var interval = Constants.DefaultInterval;
        int? count = null;
        DateTime? until = null;
        var untilIsDateOnly = false;
        IReadOnlyList<int> byMonth = Array.Empty<int>();
        IReadOnlyList<int> byMonthDay = Array.Empty<int>();
        IReadOnlyList<WeekdayNum> byDay = Array.Empty<WeekdayNum>();
        IReadOnlyList<int> byYearDay = Array.Empty<int>();
        IReadOnlyList<int> byWeekNo = Array.Empty<int>();
        IReadOnlyList<int> bySetPos = Array.Empty<int>();
        var weekStart = DayOfWeek.Monday;

        foreach (var part in body.Split(Constants.PartSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split(Constants.KeyValueSeparator);
            if (pair.Length != 2)
            {
                throw new RruleParseException(
                    string.Format(CultureInfo.InvariantCulture, "Malformed rule part '{0}'.", part));
            }

            var key = pair[0].Trim().ToUpperInvariant();
            var value = pair[1].Trim();

            switch (key)
            {
                case Constants.FreqKey:
                    frequency = ParseFrequency(value);
                    break;
                case Constants.IntervalKey:
                    interval = ParsePositiveInt(value, Constants.IntervalKey);
                    break;
                case Constants.CountKey:
                    count = ParsePositiveInt(value, Constants.CountKey);
                    break;
                case Constants.UntilKey:
                    until = ParseUntil(value, out untilIsDateOnly);
                    break;
                case Constants.ByMonthKey:
                    byMonth = ParseBoundedIntList(value, Constants.ByMonthKey, allowNegative: false, Constants.MaxMonth);
                    break;
                case Constants.ByMonthDayKey:
                    byMonthDay = ParseBoundedIntList(value, Constants.ByMonthDayKey, allowNegative: true, Constants.MaxMonthDay);
                    break;
                case Constants.ByDayKey:
                    byDay = ParseByDay(value);
                    break;
                case Constants.ByYearDayKey:
                    byYearDay = ParseBoundedIntList(value, Constants.ByYearDayKey, allowNegative: true, Constants.MaxYearDay);
                    break;
                case Constants.ByWeekNoKey:
                    byWeekNo = ParseBoundedIntList(value, Constants.ByWeekNoKey, allowNegative: true, Constants.MaxWeekNo);
                    break;
                case Constants.BySetPosKey:
                    bySetPos = ParseBoundedIntList(value, Constants.BySetPosKey, allowNegative: true, Constants.MaxSetPos);
                    break;
                case Constants.WeekStartKey:
                    weekStart = WeekdayTokens.Parse(value.ToUpperInvariant());
                    break;
                default:
                    throw new RruleParseException(
                        string.Format(CultureInfo.InvariantCulture, "Unsupported rule part '{0}'.", key));
            }
        }

        if (frequency is null)
        {
            throw new RruleParseException("The rule is missing the required FREQ part.");
        }

        if (count is not null && until is not null)
        {
            throw new RruleParseException("COUNT and UNTIL must not both be present.");
        }

        ValidateByDayOrdinals(frequency.Value, byDay, byWeekNo);
        ValidateBySetPos(bySetPos, byMonth, byMonthDay, byDay, byYearDay, byWeekNo);

        return new RecurrenceRule(
            frequency.Value, interval, count, until, untilIsDateOnly,
            byMonth, byMonthDay, byDay, byYearDay, byWeekNo, bySetPos, weekStart);
    }

    private static void ValidateByDayOrdinals(
        Frequency frequency,
        IReadOnlyList<WeekdayNum> byDay,
        IReadOnlyList<int> byWeekNo)
    {
        var hasOrdinal = byDay.Any(entry => entry.Ordinal is not null);
        if (!hasOrdinal)
        {
            return;
        }

        // RFC 5545 3.3.10: an ordinal BYDAY value only makes sense for MONTHLY or YEARLY.
        if (frequency is Frequency.Daily or Frequency.Weekly)
        {
            throw new RruleParseException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "An ordinal BYDAY value is only valid with MONTHLY or YEARLY, not {0}.",
                    frequency));
        }

        // RFC 5545 3.3.10: BYDAY MUST NOT carry an ordinal when FREQ=YEARLY and BYWEEKNO is present;
        // the week is already selected by BYWEEKNO, so an ordinal weekday within the year is undefined.
        if (frequency is Frequency.Yearly && byWeekNo.Count > 0)
        {
            throw new RruleParseException(
                "An ordinal BYDAY value cannot be combined with BYWEEKNO.");
        }
    }

    private static void ValidateBySetPos(
        IReadOnlyList<int> bySetPos,
        IReadOnlyList<int> byMonth,
        IReadOnlyList<int> byMonthDay,
        IReadOnlyList<WeekdayNum> byDay,
        IReadOnlyList<int> byYearDay,
        IReadOnlyList<int> byWeekNo)
    {
        if (bySetPos.Count == 0)
        {
            return;
        }

        var hasOtherByPart =
            byMonth.Count > 0 || byMonthDay.Count > 0 || byDay.Count > 0 ||
            byYearDay.Count > 0 || byWeekNo.Count > 0;

        if (!hasOtherByPart)
        {
            throw new RruleParseException("BYSETPOS requires at least one other BYxxx rule part.");
        }
    }

    private static string StripPrefix(string text) =>
        text.StartsWith(Constants.RulePrefix, StringComparison.OrdinalIgnoreCase)
            ? text[Constants.RulePrefix.Length..].Trim()
            : text;

    private static Frequency ParseFrequency(string value)
    {
        if (!Frequencies.TryGetValue(value.ToUpperInvariant(), out var frequency))
        {
            throw new RruleParseException(
                string.Format(CultureInfo.InvariantCulture, "Unknown FREQ value '{0}'.", value));
        }

        return frequency;
    }

    private static int ParsePositiveInt(string value, string key)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed < 1)
        {
            throw new RruleParseException(
                string.Format(CultureInfo.InvariantCulture, "{0} must be a positive integer but was '{1}'.", key, value));
        }

        return parsed;
    }

    private static DateTime ParseUntil(string value, out bool isDateOnly)
    {
        if (DateTime.TryParseExact(
                value, Constants.UntilDateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dateOnly))
        {
            isDateOnly = true;
            return dateOnly;
        }

        string[] dateTimeFormats =
        {
            Constants.UntilDateTimeUtcFormat,
            Constants.UntilDateTimeLocalFormat,
        };

        if (DateTime.TryParseExact(
                value, dateTimeFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
        {
            isDateOnly = false;
            return parsed;
        }

        throw new RruleParseException(
            string.Format(CultureInfo.InvariantCulture, "UNTIL is not a valid date/time: '{0}'.", value));
    }

    private static IReadOnlyList<int> ParseBoundedIntList(string value, string key, bool allowNegative, int maxMagnitude)
    {
        var tokens = value.Split(Constants.ValueListSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            throw new RruleParseException(
                string.Format(CultureInfo.InvariantCulture, "{0} must have at least one value.", key));
        }

        var result = new int[tokens.Length];
        for (var i = 0; i < tokens.Length; i++)
        {
            if (!int.TryParse(tokens[i].Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsed))
            {
                throw new RruleParseException(
                    string.Format(CultureInfo.InvariantCulture, "{0} contains a non-integer value '{1}'.", key, tokens[i]));
            }

            if (!allowNegative && parsed < 0)
            {
                throw new RruleParseException(
                    string.Format(CultureInfo.InvariantCulture, "{0} must not contain a negative value.", key));
            }

            var magnitude = Math.Abs(parsed);
            if (magnitude < 1 || magnitude > maxMagnitude)
            {
                throw new RruleParseException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} value '{1}' is out of the allowed range (magnitude 1 to {2}).",
                        key, parsed, maxMagnitude));
            }

            result[i] = parsed;
        }

        return result;
    }

    private static IReadOnlyList<WeekdayNum> ParseByDay(string value)
    {
        var tokens = value.Split(Constants.ValueListSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            throw new RruleParseException("BYDAY must have at least one value.");
        }

        var result = new WeekdayNum[tokens.Length];
        for (var i = 0; i < tokens.Length; i++)
        {
            result[i] = ParseWeekdayNum(tokens[i].Trim().ToUpperInvariant());
        }

        return result;
    }

    private static WeekdayNum ParseWeekdayNum(string token)
    {
        if (token.Length < 2)
        {
            throw new RruleParseException(
                string.Format(CultureInfo.InvariantCulture, "Malformed BYDAY entry '{0}'.", token));
        }

        var dayPart = token[^2..];
        var ordinalPart = token[..^2];

        var day = WeekdayTokens.Parse(dayPart);

        if (ordinalPart.Length == 0)
        {
            return new WeekdayNum(day, ordinal: null);
        }

        if (!int.TryParse(ordinalPart, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var ordinal)
            || ordinal == 0)
        {
            throw new RruleParseException(
                string.Format(CultureInfo.InvariantCulture, "Malformed BYDAY ordinal in '{0}'.", token));
        }

        return new WeekdayNum(day, ordinal);
    }
}
