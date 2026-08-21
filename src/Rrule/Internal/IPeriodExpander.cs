namespace Rrule.Internal;

/// <summary>
/// Strategy for one <see cref="Frequency"/>. Produces, for each interval period, the
/// ordered set of candidate dates (at midnight) before <c>BYSETPOS</c>, <c>UNTIL</c> and
/// <c>COUNT</c> are applied by the engine.
/// </summary>
internal interface IPeriodExpander
{
    /// <summary>The anchor (period marker) for the period that contains the start date.</summary>
    DateTime InitialAnchor(RecurrenceRule rule, DateTime start);

    /// <summary>Advances the anchor forward by the rule's interval.</summary>
    DateTime NextAnchor(RecurrenceRule rule, DateTime anchor);

    /// <summary>The candidate dates for the period identified by <paramref name="anchor"/>.</summary>
    IEnumerable<DateTime> Candidates(RecurrenceRule rule, DateTime anchor, DateTime start);
}
