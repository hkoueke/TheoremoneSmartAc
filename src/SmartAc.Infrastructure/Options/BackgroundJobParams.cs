using System.ComponentModel.DataAnnotations;

namespace SmartAc.Infrastructure.Options;

public sealed class BackgroundJobParams
{
    [Range(50, 150)]
    public int BatchSize { get; set; } = 50;

    [Range(5, 15)]
    public int IntervalInSeconds { get; set; } = 5;
}