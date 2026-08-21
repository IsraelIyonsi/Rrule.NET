using System.Globalization;

namespace Rrule.Internal;

/// <summary>
/// Maps the two-letter RFC 5545 weekday abbreviations to and from
/// <see cref="DayOfWeek"/>, without any culture dependence.
/// </summary>
internal static class WeekdayTokens
{
    public const string Sunday = "SU";
    public const string Monday = "MO";
    public const string Tuesday = "TU";
    public const string Wednesday = "WE";
    public const string Thursday = "TH";
    public const string Friday = "FR";
    public const string Saturday = "SA";

    private static readonly IReadOnlyDictionary<string, DayOfWeek> ByToken =
        new Dictionary<string, DayOfWeek>(StringComparer.Ordinal)
        {
            [Sunday] = DayOfWeek.Sunday,
            [Monday] = DayOfWeek.Monday,
            [Tuesday] = DayOfWeek.Tuesday,
            [Wednesday] = DayOfWeek.Wednesday,
            [Thursday] = DayOfWeek.Thursday,
            [Friday] = DayOfWeek.Friday,
            [Saturday] = DayOfWeek.Saturday,
        };

    public static bool TryParse(string token, out DayOfWeek day) => ByToken.TryGetValue(token, out day);

    public static DayOfWeek Parse(string token)
    {
        if (!TryParse(token, out var day))
        {
            throw new RruleParseException(
                string.Format(CultureInfo.InvariantCulture, "Unknown weekday token '{0}'.", token));
        }

        return day;
    }
}
