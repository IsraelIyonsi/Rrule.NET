namespace Rrule.Internal;

/// <summary>
/// Pure calendar arithmetic shared by the period expanders: resolving month days and
/// year days (including negative "from the end" values), enumerating weekday
/// occurrences, and aligning to the start of a week.
/// </summary>
internal static class DateHelpers
{
    public static int DaysInYear(int year) => DateTime.IsLeapYear(year) ? 366 : 365;

    /// <summary>
    /// Resolves a <c>BYMONTHDAY</c> value to an actual day number for the given month,
    /// mapping negatives from the end (-1 is the last day). Returns <see langword="null"/>
    /// when the day does not exist in that month (for example 31 in a 30-day month).
    /// </summary>
    public static int? ResolveMonthDay(int year, int month, int monthDay)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var day = monthDay > 0 ? monthDay : daysInMonth + monthDay + 1;
        return day >= Constants.FirstDayOfMonth && day <= daysInMonth ? day : null;
    }

    /// <summary>
    /// Resolves a <c>BYYEARDAY</c> value to a date, mapping negatives from the end
    /// (-1 is 31 December). Returns <see langword="null"/> when out of range.
    /// </summary>
    public static DateTime? ResolveYearDay(int year, int yearDay)
    {
        var daysInYear = DaysInYear(year);
        var ordinal = yearDay > 0 ? yearDay : daysInYear + yearDay + 1;
        if (ordinal < 1 || ordinal > daysInYear)
        {
            return null;
        }

        return new DateTime(year, Constants.JanuaryMonth, Constants.FirstDayOfMonth).AddDays(ordinal - 1);
    }

    /// <summary>Every date in the given month that falls on the given weekday, ascending.</summary>
    public static IReadOnlyList<DateTime> WeekdayOccurrencesInMonth(int year, int month, DayOfWeek day)
    {
        var result = new List<DateTime>();
        var daysInMonth = DateTime.DaysInMonth(year, month);
        for (var d = Constants.FirstDayOfMonth; d <= daysInMonth; d++)
        {
            var date = new DateTime(year, month, d);
            if (date.DayOfWeek == day)
            {
                result.Add(date);
            }
        }

        return result;
    }

    /// <summary>Every date in the given year that falls on the given weekday, ascending.</summary>
    public static IReadOnlyList<DateTime> WeekdayOccurrencesInYear(int year, DayOfWeek day)
    {
        var result = new List<DateTime>();
        var lastAlignableDay = DateTime.MaxValue.AddDays(-1);
        var cursor = new DateTime(year, Constants.JanuaryMonth, Constants.FirstDayOfMonth);
        while (cursor.DayOfWeek != day)
        {
            if (cursor >= lastAlignableDay)
            {
                return result;
            }

            cursor = cursor.AddDays(1);
        }

        var lastStrideDay = DateTime.MaxValue.AddDays(-Constants.DaysInWeek);
        while (cursor.Year == year)
        {
            result.Add(cursor);
            if (cursor > lastStrideDay)
            {
                break;
            }

            cursor = cursor.AddDays(Constants.DaysInWeek);
        }

        return result;
    }

    /// <summary>
    /// Picks a single item from an ordered occurrence list by a 1-based ordinal, where a
    /// positive value counts from the front and a negative from the back. Returns
    /// <see langword="null"/> when the ordinal falls outside the list.
    /// </summary>
    public static T? SelectByOrdinal<T>(IReadOnlyList<T> ordered, int ordinal) where T : struct
    {
        var index = ordinal > 0 ? ordinal - 1 : ordered.Count + ordinal;
        return index >= 0 && index < ordered.Count ? ordered[index] : null;
    }

    /// <summary>The most recent occurrence of <paramref name="weekStart"/> on or before the date.</summary>
    public static DateTime StartOfWeek(DateTime date, DayOfWeek weekStart)
    {
        var delta = (Constants.DaysInWeek + (int)date.DayOfWeek - (int)weekStart) % Constants.DaysInWeek;
        return date.Date.AddDays(-delta);
    }
}
