#pragma warning disable CS8618 
using System;
using System.Collections.Generic;

namespace Hashgraph.Extensions
{
    /// <summary>
    /// Object containing the current and next fee schedule
    /// returned from the network.
    /// </summary>
    public sealed class FeeSchedules
    {
        /// <summary>
        /// Internal constructor, used by the library to create an
        /// initialized exchange rates object.
        /// </summary>
        /// <param name="current">Current Exchange Rate</param>
        /// <param name="next">Next Exchange Rate</param>
        internal FeeSchedules(FeeSchedule current, FeeSchedule next)
        {
            Current = current;
            Next = next;
        }
        /// <summary>
        /// Current Fee Schedule
        /// </summary>
        public FeeSchedule Current { get; }
        /// <summary>
        /// Exchange rate that is in effect after 
        /// the current fee schedule expires.
        /// </summary>
        public FeeSchedule Next { get; }
    }
    /// <summary>
    /// Contains a dictionary holding the fee calculation
    /// parameters for various network functions.
    /// </summary>
    public sealed class FeeSchedule
    {
        /// <summary>
        /// A dictionary mapping hedera functionality (by name) to 
        /// structured fee data (serialized as JSON data).
        /// </summary>
        public Dictionary<string, string> Data { get; internal set; }
        /// <summary>
        /// The Time at which this fee schedule expires.
        /// </summary>
        public DateTime Expires { get; internal set; }
    }
}
