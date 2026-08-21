namespace Rrule.Internal;

/// <summary>
/// Applies <c>BYSETPOS</c>: from a period's ordered candidate set, keeps only the
/// nominated 1-based positions (negatives count from the end), returned in ascending order.
/// </summary>
internal static class BySetPosSelector
{
    public static IReadOnlyList<DateTime> Apply(IReadOnlyList<DateTime> ordered, IReadOnlyList<int> bySetPos)
    {
        if (bySetPos.Count == 0)
        {
            return ordered;
        }

        var picked = new SortedSet<DateTime>();
        foreach (var position in bySetPos)
        {
            var selected = DateHelpers.SelectByOrdinal(ordered, position);
            if (selected is not null)
            {
                picked.Add(selected.Value);
            }
        }

        return picked.ToArray();
    }
}
