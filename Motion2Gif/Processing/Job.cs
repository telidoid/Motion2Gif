using System;
using System.Threading;

namespace Motion2Gif.Processing;

public record Job(
    JobId Id,
    IProgress<JobProgress>? Progress,
    IJobModel Model,
    CancellationToken CancellationToken = default);