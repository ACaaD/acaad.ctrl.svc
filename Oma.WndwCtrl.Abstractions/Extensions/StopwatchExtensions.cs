using System.Diagnostics;

namespace Oma.WndwCtrl.Abstractions.Extensions;

public static class StopwatchExtensions
{
    public static TimeSpan Measure(this Stopwatch stopwatch)
    {
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}