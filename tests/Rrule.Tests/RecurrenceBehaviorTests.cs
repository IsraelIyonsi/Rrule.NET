using Rrule;

namespace Rrule.Tests;

/// <summary>
/// Behavioural guarantees beyond the exact-date vectors: UNTIL bounding, lazy unbounded
/// expansion consumed with Take, the runaway guard, and deferred-feature handling.
/// </summary>
public class RecurrenceBehaviorTests
{
    private static readonly DateTime Tuesday = new(1997, 9, 2);

    [Fact]
    public void Until_bound_is_inclusive()
    {
        var occurrences = Recurrence.Expand("FREQ=DAILY;UNTIL=19970905", Tuesday).ToArray();

        Assert.Equal(
            new[]
            {
                new DateTime(1997, 9, 2), new DateTime(1997, 9, 3),
                new DateTime(1997, 9, 4), new DateTime(1997, 9, 5),
            },
            occurrences);
    }

    [Fact]
    public void Date_only_until_is_inclusive_of_the_whole_day_when_dtstart_has_a_time()
    {
        var start = Tuesday.AddHours(9);

        var occurrences = Recurrence.Expand("FREQ=DAILY;UNTIL=19970905", start).ToArray();

        Assert.Equal(
            new[] { start, start.AddDays(1), start.AddDays(2), start.AddDays(3) },
            occurrences);
    }

    [Fact]
    public void Until_with_time_is_respected()
    {
        var start = Tuesday.AddHours(9);
        var occurrences = Recurrence.Expand("FREQ=DAILY;UNTIL=19970904T090000", start).ToArray();

        Assert.Equal(
            new[] { start, start.AddDays(1), start.AddDays(2) },
            occurrences);
    }

    [Fact]
    public void Unbounded_rule_is_lazy_and_takeable()
    {
        var first5 = Recurrence.Expand("FREQ=DAILY", Tuesday).Take(5).ToArray();

        Assert.Equal(
            new[]
            {
                new DateTime(1997, 9, 2), new DateTime(1997, 9, 3), new DateTime(1997, 9, 4),
                new DateTime(1997, 9, 5), new DateTime(1997, 9, 6),
            },
            first5);
    }

    [Fact]
    public void Unbounded_monthly_ordinal_is_takeable()
    {
        var first3 = Recurrence.Expand("FREQ=MONTHLY;BYDAY=1MO", new DateTime(1997, 9, 1))
            .Take(3)
            .ToArray();

        Assert.Equal(
            new[] { new DateTime(1997, 9, 1), new DateTime(1997, 10, 6), new DateTime(1997, 11, 3) },
            first3);
    }

    [Fact]
    public void Occurrence_carries_the_start_time_of_day()
    {
        var start = new DateTime(1997, 9, 2, 14, 30, 0);

        var second = Recurrence.Expand("FREQ=DAILY;COUNT=2", start).Last();

        Assert.Equal(new DateTime(1997, 9, 3, 14, 30, 0), second);
    }

    [Fact]
    public void Impossible_rule_terminates_cleanly_rather_than_looping_forever()
    {
        // 30 February never exists, so every yearly period is empty; the guard stops enumeration.
        var occurrences = Recurrence.Expand("FREQ=YEARLY;BYMONTH=2;BYMONTHDAY=30", new DateTime(2000, 1, 1))
            .Take(1)
            .ToArray();

        Assert.Empty(occurrences);
    }

    [Fact]
    public void Sparse_leap_day_rule_skips_many_empty_periods_without_hitting_the_guard()
    {
        var occurrences = Recurrence.Expand("FREQ=YEARLY;BYMONTH=2;BYMONTHDAY=29", new DateTime(2020, 2, 29))
            .Take(4)
            .ToArray();

        Assert.Equal(
            new[]
            {
                new DateTime(2020, 2, 29), new DateTime(2024, 2, 29),
                new DateTime(2028, 2, 29), new DateTime(2032, 2, 29),
            },
            occurrences);
    }

    [Fact]
    public void Sparse_year_day_366_rule_only_yields_leap_years()
    {
        var occurrences = Recurrence.Expand("FREQ=YEARLY;BYYEARDAY=366", new DateTime(2020, 1, 1))
            .Take(3)
            .ToArray();

        Assert.Equal(
            new[]
            {
                new DateTime(2020, 12, 31), new DateTime(2024, 12, 31), new DateTime(2028, 12, 31),
            },
            occurrences);
    }

    [Theory]
    [InlineData("FREQ=HOURLY;COUNT=3")]
    [InlineData("FREQ=MINUTELY;COUNT=3")]
    [InlineData("FREQ=SECONDLY;COUNT=3")]
    public void Sub_daily_frequencies_are_deferred(string rrule)
    {
        Assert.Throws<NotSupportedException>(() => Recurrence.Expand(rrule, Tuesday).ToArray());
    }

    [Fact]
    public void ByWeekNo_with_non_monday_week_start_is_deferred()
    {
        Assert.Throws<NotSupportedException>(
            () => Recurrence.Expand("FREQ=YEARLY;BYWEEKNO=20;BYDAY=MO;WKST=SU", Tuesday).ToArray());
    }

    [Fact]
    public void ByWeekNo_with_non_yearly_frequency_is_deferred()
    {
        Assert.Throws<NotSupportedException>(
            () => Recurrence.Expand("FREQ=WEEKLY;BYWEEKNO=20", Tuesday).ToArray());
    }
}
