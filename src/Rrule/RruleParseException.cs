namespace Rrule;

/// <summary>
/// Thrown when an RRULE string is malformed or contains a value that violates
/// RFC 5545, for example a missing <c>FREQ</c>, an unknown rule part, a bad
/// weekday token, or both <c>COUNT</c> and <c>UNTIL</c> present at once.
/// </summary>
public sealed class RruleParseException : Exception
{
    /// <summary>Creates the exception with a human-readable message.</summary>
    /// <param name="message">Description of what was wrong with the rule.</param>
    public RruleParseException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and an underlying cause.</summary>
    /// <param name="message">Description of what was wrong with the rule.</param>
    /// <param name="innerException">The underlying error that triggered this one.</param>
    public RruleParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
