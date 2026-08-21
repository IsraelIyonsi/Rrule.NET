using System.Globalization;
using Rrule;

namespace Rrule.Tests;

/// <summary>
/// The correctness oracle. Each row is a worked RRULE example from RFC 5545 section 3.8.5.3
/// (or a direct, hand-derived variation of one) with its exact expected occurrence dates.
/// The full produced sequence is asserted element for element. These vectors are authoritative:
/// they were computed from the calendar, not from the implementation.
/// </summary>
public class RfcExampleVectors
{
    private const int NineAm = 9;
    private const string DateFormat = "yyyy-MM-dd";

    public static TheoryData<string, string, string, string[]> Vectors => new()
    {
        // Daily for 10 occurrences.
        {
            "1997-09-02", "FREQ=DAILY;COUNT=10",
            "daily count 10",
            new[]
            {
                "1997-09-02", "1997-09-03", "1997-09-04", "1997-09-05", "1997-09-06",
                "1997-09-07", "1997-09-08", "1997-09-09", "1997-09-10", "1997-09-11",
            }
        },

        // Weekly for 10 occurrences (keeps the start weekday, Tuesday).
        {
            "1997-09-02", "FREQ=WEEKLY;COUNT=10",
            "weekly count 10",
            new[]
            {
                "1997-09-02", "1997-09-09", "1997-09-16", "1997-09-23", "1997-09-30",
                "1997-10-07", "1997-10-14", "1997-10-21", "1997-10-28", "1997-11-04",
            }
        },

        // Weekly on Tuesday and Thursday, WKST=SU, 10 occurrences (BYDAY expansion in a week).
        {
            "1997-09-02", "FREQ=WEEKLY;COUNT=10;WKST=SU;BYDAY=TU,TH",
            "weekly tu,th count 10",
            new[]
            {
                "1997-09-02", "1997-09-04", "1997-09-09", "1997-09-11", "1997-09-16",
                "1997-09-18", "1997-09-23", "1997-09-25", "1997-09-30", "1997-10-02",
            }
        },

        // Monthly on the first and last day of the month for 10 occurrences (BYMONTHDAY with -1).
        {
            "1997-09-30", "FREQ=MONTHLY;BYMONTHDAY=1,-1;COUNT=10",
            "monthly first and last day",
            new[]
            {
                "1997-09-30", "1997-10-01", "1997-10-31", "1997-11-01", "1997-11-30",
                "1997-12-01", "1997-12-31", "1998-01-01", "1998-01-31", "1998-02-01",
            }
        },

        // Monthly on the first Friday for 10 occurrences (ordinal BYDAY 1FR).
        {
            "1997-09-05", "FREQ=MONTHLY;BYDAY=1FR;COUNT=10",
            "monthly first friday",
            new[]
            {
                "1997-09-05", "1997-10-03", "1997-11-07", "1997-12-05", "1998-01-02",
                "1998-02-06", "1998-03-06", "1998-04-03", "1998-05-01", "1998-06-05",
            }
        },

        // Monthly on the last Sunday for 6 occurrences (negative ordinal BYDAY -1SU).
        {
            "1997-09-28", "FREQ=MONTHLY;BYDAY=-1SU;COUNT=6",
            "monthly last sunday",
            new[]
            {
                "1997-09-28", "1997-10-26", "1997-11-30", "1997-12-28", "1998-01-25", "1998-02-22",
            }
        },

        // Last Tuesday/Wednesday/Thursday of each month via BYSETPOS=-1, 6 occurrences.
        // December 1997 resolves to the 31st (a Wednesday, in the TU/WE/TH set).
        {
            "1997-09-02", "FREQ=MONTHLY;COUNT=6;BYDAY=TU,WE,TH;BYSETPOS=-1",
            "monthly last tu/we/th (bysetpos -1)",
            new[]
            {
                "1997-09-30", "1997-10-30", "1997-11-27", "1997-12-31", "1998-01-29", "1998-02-26",
            }
        },

        // Yearly in January on every Sunday, 4 occurrences (YEARLY BYMONTH + plain BYDAY expand).
        {
            "1998-01-04", "FREQ=YEARLY;BYMONTH=1;BYDAY=SU;COUNT=4",
            "yearly sundays in january",
            new[]
            {
                "1998-01-04", "1998-01-11", "1998-01-18", "1998-01-25",
            }
        },

        // Monthly on the 31st for 5 occurrences, skipping months without a 31st.
        {
            "1998-01-31", "FREQ=MONTHLY;BYMONTHDAY=31;COUNT=5",
            "monthly on the 31st (skips short months)",
            new[]
            {
                "1998-01-31", "1998-03-31", "1998-05-31", "1998-07-31", "1998-08-31",
            }
        },

        // Yearly on 29 February for 3 occurrences, only in leap years.
        {
            "2020-02-29", "FREQ=YEARLY;COUNT=3",
            "yearly feb 29 (leap years only)",
            new[]
            {
                "2020-02-29", "2024-02-29", "2028-02-29",
            }
        },

        // Every other month (INTERVAL) on the first Monday, 4 occurrences.
        {
            "1997-09-01", "FREQ=MONTHLY;INTERVAL=2;BYDAY=1MO;COUNT=4",
            "every other month first monday",
            new[]
            {
                "1997-09-01", "1997-11-03", "1998-01-05", "1998-03-02",
            }
        },
    };

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Expands_to_exact_rfc_dates(string dtstart, string rrule, string label, string[] expectedDates)
    {
        _ = label;
        var start = ParseDate(dtstart).AddHours(NineAm);
        var expected = expectedDates.Select(d => ParseDate(d).AddHours(NineAm)).ToArray();

        var actual = Recurrence.Expand(rrule, start).ToArray();

        Assert.Equal(expected, actual);
    }

    private static DateTime ParseDate(string value) =>
        DateTime.ParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
}
