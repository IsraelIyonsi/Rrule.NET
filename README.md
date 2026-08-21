# Rrule.Net

A focused, zero-dependency RFC 5545 `RRULE` expander for .NET. Give it a recurrence rule and a start date; get back the occurrence dates. Nothing else.

Built for the recurring-date questions that show up in real software: billing cycles, dunning schedules, subscription renewals, reminders. Deterministic, lazy, offline, AOT-friendly.

## Why this exists

The maintained incumbent for iCalendar work on .NET is [Ical.Net](https://github.com/ical-org/ical.net), and it is excellent. But it is a full iCalendar stack: calendars, events, alarms, time zones, serialization. If all you need is "expand this one `RRULE` into dates," that is a large surface to take on as a dependency.

`Rrule.Net` is the small wedge under that: one job, done correctly, with no dependencies and a body of RFC 5545 worked-example test vectors backing the result. If you later need full iCalendar support, reach for Ical.Net. If you just need occurrences from a rule, this stays out of your way.

## Install

```
dotnet add package Rrule.Net
```

Targets `net8.0`. Zero runtime dependencies. `IsAotCompatible`.

## Quick start

```csharp
using Rrule;

// Parse-and-expand in one call.
foreach (var date in Recurrence.Expand("FREQ=MONTHLY;BYDAY=1FR;COUNT=10", new DateTime(1997, 9, 5)))
{
    Console.WriteLine(date.ToString("yyyy-MM-dd"));
}
// 1997-09-05, 1997-10-03, 1997-11-07, 1997-12-05, 1998-01-02, ...
```

Expansion is lazy, so an unbounded rule (no `COUNT`, no `UNTIL`) is safe to consume with `Take`:

```csharp
var nextFive = Recurrence.Expand("FREQ=WEEKLY;INTERVAL=2;BYDAY=MO", DateTime.Today)
    .Take(5)
    .ToList();
```

You can also parse once and reuse the rule:

```csharp
RecurrenceRule rule = RecurrenceRule.Parse("FREQ=YEARLY;BYMONTH=1;BYDAY=SU;COUNT=4");
IEnumerable<DateTime> sundays = Recurrence.Expand(rule, new DateTime(1998, 1, 4));
```

The start date is the `DTSTART`. Its time of day is carried onto every occurrence, and it is the lower bound: occurrences earlier than it are never produced. A malformed rule throws `RruleParseException`.

## Supported rule parts

| Part | Support |
| --- | --- |
| `FREQ` | `DAILY`, `WEEKLY`, `MONTHLY`, `YEARLY` |
| `INTERVAL` | Yes (default 1) |
| `COUNT` | Yes |
| `UNTIL` | Yes (inclusive; date or date-time, `Z` accepted). A date-only `UNTIL` is inclusive of the whole final day, so occurrences on that date are kept regardless of their time of day. |
| `BYMONTH` | Yes |
| `BYMONTHDAY` | Yes, including negatives (`-1` = last day of month) |
| `BYDAY` | Yes, plain (`MO`) and ordinal (`1MO`, `-1FR`) |
| `BYSETPOS` | Yes, including negatives |
| `BYYEARDAY` | Yes, including negatives |
| `WKST` | Yes (default `MO`; affects `WEEKLY` period boundaries) |

### Deferred (conscious limitations, not silent gaps)

| Part | Status |
| --- | --- |
| `FREQ=HOURLY` / `MINUTELY` / `SECONDLY` | Parsed, but expansion throws `NotSupportedException`. This library is date-grained. |
| `BYWEEKNO` | Parsed, but expansion throws `NotSupportedException`. ISO week-number expansion is not yet implemented. |
| `BYHOUR` / `BYMINUTE` / `BYSECOND` | Not recognized (time-of-day comes from `DTSTART`); rejected at parse time. |

If a rule uses a deferred feature you get a clear, typed failure rather than a wrong answer.

### Validation

Rules are validated up front, so a bad rule fails as `RruleParseException` at `Parse` time rather than surfacing an obscure error mid-enumeration. This includes out-of-range values (`BYMONTH` outside 1 to 12, `BYMONTHDAY` outside 1 to 31 or -1 to -31, `BYYEARDAY`, `BYWEEKNO`, `BYSETPOS`), an ordinal `BYDAY` such as `1MO` used with `DAILY` or `WEEKLY` (RFC 5545 allows the numeric form only with `MONTHLY` or `YEARLY`), `BYSETPOS` without any other `BYxxx` part, and `COUNT` together with `UNTIL`.

## The runaway guard

Some rules can never produce an occurrence (for example `FREQ=YEARLY;BYMONTH=2;BYMONTHDAY=30` asks for 30 February). Because expansion is lazy and can be unbounded, fully enumerating such a rule could otherwise loop indefinitely.

The engine counts consecutive periods that yield nothing. After `100,000` empty periods in a row it stops cleanly (the sequence simply ends). Enumeration also ends cleanly if it walks off the end of the representable calendar (`DateTime.MaxValue`). A valid rule is never affected, because it resets the counter every time it produces a date.

## Correctness: the RFC 5545 test vectors

Correctness is the whole product, so the test suite asserts exact occurrence dates against worked examples from [RFC 5545 section 3.8.5.3](https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.5.3) and hand-derived variations of them. The full produced sequence is compared element for element. The vectors currently asserted include:

- `FREQ=DAILY;COUNT=10`
- `FREQ=WEEKLY;COUNT=10`
- `FREQ=WEEKLY;COUNT=10;WKST=SU;BYDAY=TU,TH`
- `FREQ=MONTHLY;BYMONTHDAY=1,-1;COUNT=10` (first and last day of month)
- `FREQ=MONTHLY;BYDAY=1FR;COUNT=10` (first Friday)
- `FREQ=MONTHLY;BYDAY=-1SU;COUNT=6` (last Sunday)
- `FREQ=MONTHLY;COUNT=6;BYDAY=TU,WE,TH;BYSETPOS=-1` (last Tue/Wed/Thu of month)
- `FREQ=YEARLY;BYMONTH=1;BYDAY=SU;COUNT=4` (Sundays in January)
- `FREQ=MONTHLY;BYMONTHDAY=31;COUNT=5` (skips months without a 31st)
- `FREQ=YEARLY;COUNT=3` from 29 February (leap years only)
- `FREQ=MONTHLY;INTERVAL=2;BYDAY=1MO;COUNT=4` (every other month, first Monday)

plus behavioural tests for `UNTIL` bounding, lazy `Take` on unbounded rules, time-of-day propagation, the runaway guard, and deferred-feature and malformed-rule failures.

Run them:

```
dotnet test tests/Rrule.Tests/Rrule.Tests.csproj -c Release
```

## License

MIT (c) Israel Iyonsi. See [LICENSE](LICENSE).
