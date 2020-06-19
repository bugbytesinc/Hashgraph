using System;

namespace Proto
{
    public sealed partial class Duration
    {
        internal Duration(TimeSpan timespan) : this()
        {
            Seconds = (long)timespan.TotalSeconds;
        }
        internal TimeSpan ToTimeSpan()
        {
            return TimeSpan.FromSeconds(Seconds);
        }
    }
}
