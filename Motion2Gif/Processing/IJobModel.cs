using System;

namespace Motion2Gif.Processing;

public interface IJobModel
{
    public string OutputFilePath { get; init; }
};

public record GenerateGifJob(MediaRange MediaRange, Uri FilePath, string OutputFilePath) : IJobModel;

public record CutVideoJob(MediaRange MediaRange, Uri FilePath, string OutputFilePath) : IJobModel;