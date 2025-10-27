using System;

namespace Motion2Gif.Processing;

public readonly record struct JobId(Guid Value)
{
    public static JobId Create() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}