using System.Diagnostics;

namespace NabeatsuGenerator;

[Conditional("NABEATSU")]
[AttributeUsage(AttributeTargets.Method)]
public sealed class NabeatsuAttribute :
    Attribute
{
    public NabeatsuAttribute(
        int start,
        int end)
    {
        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        if (end < start)
        {
            throw new ArgumentOutOfRangeException(nameof(end));
        }

        this.Start = start;
        this.End = end;
    }

    public int Start { get; }

    public int End { get; }
}
