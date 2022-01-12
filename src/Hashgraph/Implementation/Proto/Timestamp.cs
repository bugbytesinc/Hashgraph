using Hashgraph.Implementation;
using System;

namespace Proto;

public sealed partial class Timestamp
{
    internal Timestamp(DateTime dateTime) : this()
    {
        (Seconds, Nanos) = Epoch.FromDate(dateTime);
    }
    internal DateTime ToDateTime()
    {
        return Epoch.ToDate(Seconds, Nanos);
    }
}