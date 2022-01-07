using Hashgraph.Implementation;
using System;

namespace Proto;

public sealed partial class TimestampSeconds
{
    internal DateTime ToDateTime()
    {
        return Epoch.ToDate(Seconds, 0);
    }
}