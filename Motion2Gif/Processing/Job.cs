using System;
using System.Threading;

namespace Motion2Gif.Processing;

public record Job(
    JobId Id,
    IJobModel Model,
    CancellationTokenSource CancellationTokenSource,
    CancellationToken CancellationToken = default);