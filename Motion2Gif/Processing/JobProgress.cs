using System;

namespace Motion2Gif.Processing;

public record JobProgress(double Percent, TimeSpan Processed, TimeSpan Total, JobState State, string? Phase);