namespace Rrule.Internal;

/// <summary>
/// Named literals for the RRULE grammar and the expansion engine, so that no magic
/// numbers or strings appear in the parsing and generation code.
/// </summary>
internal static class Constants
{
    public const int DefaultInterval = 1;
    public const int DaysInWeek = 7;
    public const int MonthsInYear = 12;
    public const int FirstDayOfMonth = 1;
    public const int JanuaryMonth = 1;

    public const int MaxMonth = 12;
    public const int MaxMonthDay = 31;
    public const int MaxYearDay = 366;
    public const int MaxWeekNo = 53;
    public const int MaxSetPos = 366;

    public const char PartSeparator = ';';
    public const char KeyValueSeparator = '=';
    public const char ValueListSeparator = ',';
    public const string RulePrefix = "RRULE:";

    public const string FreqKey = "FREQ";
    public const string IntervalKey = "INTERVAL";
    public const string CountKey = "COUNT";
    public const string UntilKey = "UNTIL";
    public const string ByMonthKey = "BYMONTH";
    public const string ByMonthDayKey = "BYMONTHDAY";
    public const string ByDayKey = "BYDAY";
    public const string ByYearDayKey = "BYYEARDAY";
    public const string ByWeekNoKey = "BYWEEKNO";
    public const string BySetPosKey = "BYSETPOS";
    public const string WeekStartKey = "WKST";

    public const string UntilDateFormat = "yyyyMMdd";
    public const string UntilDateTimeUtcFormat = "yyyyMMdd'T'HHmmss'Z'";
    public const string UntilDateTimeLocalFormat = "yyyyMMdd'T'HHmmss";

    /// <summary>
    /// Safety cap on how many consecutive interval periods may be scanned without
    /// producing a single occurrence before lazy expansion stops. This protects a
    /// caller who fully enumerates a rule that can never match (for example a
    /// 29 February yearly rule combined with a non-leap constraint) from an
    /// unbounded loop, while never affecting a valid rule.
    /// </summary>
    public const int MaxEmptyPeriods = 100_000;
}
