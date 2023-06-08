using System;
using System.Threading;

namespace Hashgraph.Implementation;

/// <summary>
/// Helper class providing .net to Hedera timestamp and 
/// timespan conversions.  Also provides a timestamp
/// creation method that guarantees a unique timestamp
/// for each call.  This helps reduce the chance of a
/// transaction ID collision under load.
/// </summary>
internal static class Epoch
{
    private const long NanosPerTick = 1_000_000_000L / TimeSpan.TicksPerSecond;
    private static long _lastValue = (DateTime.UtcNow - DateTime.UnixEpoch).Ticks * NanosPerTick;
    private static long _localClockDrift = 0;
    internal static long UniqueClockNanos()
    {
        long newValue;
        long oldValue;
        long clockValue = (DateTime.UtcNow - DateTime.UnixEpoch).Ticks * NanosPerTick;
        do
        {
            oldValue = Interlocked.Read(ref _lastValue);
            Interlocked.MemoryBarrierProcessWide();
            newValue = clockValue > oldValue ? clockValue : oldValue + 1;
        } while (oldValue != Interlocked.CompareExchange(ref _lastValue, newValue, oldValue));
        return newValue;
    }
    internal static long UniqueClockNanosAfterDrift()
    {
        return UniqueClockNanos() - Interlocked.Read(ref _localClockDrift);
    }
    internal static (long seconds, int nanos) UniqueSecondsAndNanos(bool adjustForDrift)
    {
        var total = adjustForDrift ? UniqueClockNanosAfterDrift() : UniqueClockNanos();
        return (total / 1_000_000_000L, (int)(total % 1_000_000_000L));
    }
    internal static (long seconds, int nanos) FromDate(DateTime dateTime)
    {
        TimeSpan timespan = dateTime - DateTime.UnixEpoch;
        long seconds = (long)timespan.TotalSeconds;
        int nanos = (int)((timespan.Ticks - (seconds * TimeSpan.TicksPerSecond)) * NanosPerTick);
        return (seconds, nanos);
    }
    internal static void AddToClockDrift(long additionalDrift)
    {
        Interlocked.Add(ref _localClockDrift, additionalDrift);
    }
}