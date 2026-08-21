using System.Globalization;
using Rrule.Internal;

namespace Rrule;

/// <summary>
/// Expands an RFC 5545 <see cref="RecurrenceRule"/> into its occurrence dates.
/// Expansion is lazy: an unbounded rule can be consumed safely with
/// <see cref="System.Linq.Enumerable.Take{TSource}(IEnumerable{TSource}, int)"/>.
/// </summary>
public static class Recurrence
{
    /// <summary>Expands a parsed rule from the given start date.</summary>
    /// <param name="rule">The recurrence rule.</param>
    /// <param name="start">
    /// The <c>DTSTART</c>. Its time of day is carried onto every occurrence, and it is the
    /// lower bound: occurrences earlier than it are not produced.
    /// </param>
    /// <returns>A lazy, ascending sequence of occurrences.</returns>
    /// <exception cref="NotSupportedException">
    /// The rule uses a deferred feature: a sub-daily <c>FREQ</c>, or a <c>BYWEEKNO</c>
    /// combined with a non-<c>YEARLY</c> frequency or a non-Monday <c>WKST</c>.
    /// </exception>
    public static IEnumerable<DateTime> Expand(RecurrenceRule rule, DateTime start)
    {
        ArgumentNullException.ThrowIfNull(rule);
        Guard(rule);

        var expander = ExpanderFor(rule.Frequency);
        var timeOfDay = start.TimeOfDay;
        var anchor = expander.InitialAnchor(rule, start);
        var emitted = 0;
        var emptyPeriods = 0;

        while (true)
        {
            var candidates = expander.Candidates(rule, anchor, start)
                .Distinct()
                .OrderBy(date => date)
                .ToArray();

            var selected = BySetPosSelector.Apply(candidates, rule.BySetPos);

            var producedThisPeriod = false;
            foreach (var date in selected)
            {
                var occurrence = date + timeOfDay;
                if (occurrence < start)
                {
                    continue;
                }

                if (rule.Until is not null && IsPastUntil(occurrence, rule))
                {
                    yield break;
                }

                yield return occurrence;
                producedThisPeriod = true;
                emitted++;

                if (rule.Count is not null && emitted >= rule.Count.Value)
                {
                    yield break;
                }
            }

            emptyPeriods = producedThisPeriod ? 0 : emptyPeriods + 1;
            if (emptyPeriods >= Constants.MaxEmptyPeriods)
            {
                yield break;
            }

            var overflowed = false;
            DateTime next;
            try
            {
                next = expander.NextAnchor(rule, anchor);
            }
            catch (ArgumentOutOfRangeException)
            {
                next = anchor;
                overflowed = true;
            }

            if (overflowed)
            {
                yield break;
            }

            anchor = next;
        }
    }

    /// <summary>Parses an RRULE string and expands it from the given start date.</summary>
    /// <param name="rrule">The RFC 5545 rule text.</param>
    /// <param name="start">The <c>DTSTART</c>.</param>
    /// <returns>A lazy, ascending sequence of occurrences.</returns>
    /// <exception cref="RruleParseException">The rule is malformed.</exception>
    /// <exception cref="NotSupportedException">The rule uses a deferred feature.</exception>
    public static IEnumerable<DateTime> Expand(string rrule, DateTime start) =>
        Expand(RecurrenceRule.Parse(rrule), start);

    private static bool IsPastUntil(DateTime occurrence, RecurrenceRule rule)
    {
        var until = rule.Until!.Value;
        return rule.UntilIsDateOnly ? occurrence.Date > until.Date : occurrence > until;
    }

    private static void Guard(RecurrenceRule rule)
    {
        if (rule.Frequency is Frequency.Secondly or Frequency.Minutely or Frequency.Hourly)
        {
            throw new NotSupportedException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Sub-daily frequency {0} is parsed but not yet supported by expansion.",
                    rule.Frequency));
        }

        if (rule.ByWeekNo.Count == 0)
        {
            return;
        }

        if (rule.Frequency is not Frequency.Yearly)
        {
            throw new NotSupportedException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "BYWEEKNO is supported only with FREQ=YEARLY, not {0}.",
                    rule.Frequency));
        }

        if (rule.WeekStart is not DayOfWeek.Monday)
        {
            throw new NotSupportedException(
                "BYWEEKNO currently supports the ISO 8601 default WKST=MO only.");
        }
    }

    private static IPeriodExpander ExpanderFor(Frequency frequency) => frequency switch
    {
        Frequency.Daily => new DailyExpander(),
        Frequency.Weekly => new WeeklyExpander(),
        Frequency.Monthly => new MonthlyExpander(),
        Frequency.Yearly => new YearlyExpander(),
        _ => throw new NotSupportedException(
            string.Format(CultureInfo.InvariantCulture, "Frequency {0} is not supported.", frequency)),
    };
}
