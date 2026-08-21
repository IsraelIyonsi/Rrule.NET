using Rrule;

namespace Rrule.Tests;

/// <summary>Parsing and validation of the RRULE grammar, including the malformed-rule failures.</summary>
public class ParserTests
{
    [Fact]
    public void Parses_all_supported_parts()
    {
        var rule = RecurrenceRule.Parse("RRULE:FREQ=MONTHLY;INTERVAL=2;BYDAY=1MO,-1FR,TU;BYMONTHDAY=1,-1;BYMONTH=3,6;BYSETPOS=-1;WKST=SU;COUNT=5");

        Assert.Equal(Frequency.Monthly, rule.Frequency);
        Assert.Equal(2, rule.Interval);
        Assert.Equal(5, rule.Count);
        Assert.Null(rule.Until);
        Assert.Equal(new[] { 3, 6 }, rule.ByMonth);
        Assert.Equal(new[] { 1, -1 }, rule.ByMonthDay);
        Assert.Equal(new[] { -1 }, rule.BySetPos);
        Assert.Equal(DayOfWeek.Sunday, rule.WeekStart);
        Assert.Equal(3, rule.ByDay.Count);
        Assert.Equal(new WeekdayNum(DayOfWeek.Monday, 1), rule.ByDay[0]);
        Assert.Equal(new WeekdayNum(DayOfWeek.Friday, -1), rule.ByDay[1]);
        Assert.Equal(new WeekdayNum(DayOfWeek.Tuesday, null), rule.ByDay[2]);
    }

    [Fact]
    public void Interval_defaults_to_one_and_week_start_to_monday()
    {
        var rule = RecurrenceRule.Parse("FREQ=DAILY");

        Assert.Equal(1, rule.Interval);
        Assert.Equal(DayOfWeek.Monday, rule.WeekStart);
    }

    [Fact]
    public void Parses_until_date_only_and_utc_forms()
    {
        Assert.Equal(new DateTime(1997, 12, 24), RecurrenceRule.Parse("FREQ=DAILY;UNTIL=19971224").Until);
        Assert.Equal(new DateTime(1997, 12, 24, 0, 0, 0), RecurrenceRule.Parse("FREQ=DAILY;UNTIL=19971224T000000Z").Until);
    }

    [Theory]
    [InlineData("INTERVAL=2")]
    [InlineData("FREQ=WHENEVER")]
    [InlineData("FREQ=DAILY;COUNT=3;UNTIL=19971224")]
    [InlineData("FREQ=DAILY;INTERVAL=0")]
    [InlineData("FREQ=MONTHLY;BYDAY=1XX")]
    [InlineData("FREQ=MONTHLY;BYMONTHDAY=0")]
    [InlineData("FREQ=DAILY;FOO=BAR")]
    [InlineData("FREQ=MONTHLY;BYDAY=0MO")]
    [InlineData("")]
    public void Rejects_malformed_rules(string rrule)
    {
        Assert.Throws<RruleParseException>(() => RecurrenceRule.Parse(rrule));
    }

    [Theory]
    [InlineData("FREQ=YEARLY;BYMONTH=13")]
    [InlineData("FREQ=YEARLY;BYMONTH=0")]
    [InlineData("FREQ=MONTHLY;BYMONTHDAY=32")]
    [InlineData("FREQ=MONTHLY;BYMONTHDAY=-32")]
    [InlineData("FREQ=YEARLY;BYYEARDAY=367")]
    [InlineData("FREQ=YEARLY;BYWEEKNO=54")]
    [InlineData("FREQ=MONTHLY;BYDAY=TU;BYSETPOS=400")]
    public void Rejects_out_of_range_by_parts_at_parse(string rrule)
    {
        Assert.Throws<RruleParseException>(() => RecurrenceRule.Parse(rrule));
    }

    [Theory]
    [InlineData("FREQ=DAILY;BYDAY=1MO")]
    [InlineData("FREQ=WEEKLY;BYDAY=1MO,TH")]
    [InlineData("FREQ=WEEKLY;BYDAY=-2FR")]
    public void Rejects_ordinal_byday_with_daily_or_weekly(string rrule)
    {
        Assert.Throws<RruleParseException>(() => RecurrenceRule.Parse(rrule));
    }

    [Theory]
    [InlineData("FREQ=MONTHLY;BYSETPOS=-1")]
    [InlineData("FREQ=DAILY;BYSETPOS=1")]
    public void Rejects_bysetpos_without_another_by_part(string rrule)
    {
        Assert.Throws<RruleParseException>(() => RecurrenceRule.Parse(rrule));
    }

    [Fact]
    public void Tracks_date_only_until()
    {
        Assert.True(RecurrenceRule.Parse("FREQ=DAILY;UNTIL=19971224").UntilIsDateOnly);
        Assert.False(RecurrenceRule.Parse("FREQ=DAILY;UNTIL=19971224T000000Z").UntilIsDateOnly);
    }
}
