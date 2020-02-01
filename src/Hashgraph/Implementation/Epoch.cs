using System;
using System.Threading;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Helper class providing .net to Hedera timestamp and 
    /// timespan conversions.  Also provides a timestamp
    /// creation method that guarantees a unique timestamp
    /// for each call.  This helps reduce the chance of a
    /// transaction ID collision under load.
    /// </summary>
    internal static class Epoch
    {
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly long NanosPerTick = 1_000_000_000L / TimeSpan.TicksPerSecond;
        private static long _previousNano = 0;
        private static long _localClockDrift = 0;
        internal static long UniqueClockNanos()
        {
            for (int i = 0; i < 1000; i++)
            {
                var previousValue = Interlocked.Read(ref _previousNano);
                var nanos = (DateTime.UtcNow - EPOCH).Ticks * NanosPerTick;
                if (nanos > previousValue)
                {
                    if(previousValue == Interlocked.CompareExchange(ref _previousNano, nanos, previousValue))
                    {
                        return nanos;
                    }
                }
                Thread.Yield();
            }
            throw new InvalidOperationException("Unable to retrieve a unique timestamp for a transaction, is my processor overloaded?");
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
            TimeSpan timespan = dateTime - EPOCH;
            long seconds = (long)timespan.TotalSeconds;
            int nanos = (int)((timespan.Ticks - (seconds * TimeSpan.TicksPerSecond)) * NanosPerTick);
            return (seconds, nanos);
        }
        internal static DateTime ToDate(long seconds, int nanos)
        {
            return EPOCH.AddTicks(seconds * TimeSpan.TicksPerSecond + nanos / NanosPerTick);
        }
        internal static void AddToClockDrift(long additionalDrift)
        {
            Interlocked.Add(ref _localClockDrift, additionalDrift);
        }
    }
}
