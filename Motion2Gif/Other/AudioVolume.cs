using System;

namespace Motion2Gif.Other;

public record struct AudioVolume(int Value)
{
    public static AudioVolume Create(int value) => 
        new (Math.Clamp(value, 0, 100));
}