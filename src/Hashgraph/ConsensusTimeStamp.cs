using Hashgraph.Converters;
using System;
using System.Text.Json.Serialization;

namespace Hashgraph;
/// <summary>
/// Represents a consensus timestamp value to the
/// resolution of nanoseconds.
/// </summary>
[JsonConverter(typeof(ConsensusTimeStampConverter))]
public readonly record struct ConsensusTimeStamp : IComparable<ConsensusTimeStamp>, IComparable
{
    /// <summary>
    /// The minimum possible time value.
    /// </summary>
    public static readonly ConsensusTimeStamp MinValue = new(decimal.MinValue);
    /// <summary>
    /// The maximum possible time value.
    /// </summary>
    public static readonly ConsensusTimeStamp MaxValue = new(decimal.MaxValue);
    /// <summary>
    /// The number of decimal seconds since the unix epoch.
    /// </summary>
    public decimal Seconds { get; private init; }
    /// <summary>
    /// Default Constructor, represents the minimum allowed time.
    /// </summary>
    public ConsensusTimeStamp() : this(decimal.MinValue) { }
    /// <summary>
    /// Constructor taking the number of seconds since the unix epoch.
    /// </summary>
    /// <param name="seconds">
    /// The fractional number of seconds since the unix epoch.
    /// </param>
    public ConsensusTimeStamp(decimal seconds)
    {
        Seconds = seconds;
    }
    /// <summary>
    /// Constructor taking the number of whole seconds and nano 
    /// seconds since the unix epoch.
    /// </summary>
    /// <param name="seconds">
    /// Number of complete seconds since the start of the epoch
    /// </param>
    /// <param name="nanos">
    /// Number of nanoseconds since the start of the last second
    /// </param>
    public ConsensusTimeStamp(long seconds, int nanos)
    {
        Seconds = seconds >= 0 ? decimal.Add(seconds, decimal.Divide(nanos, 1000000000m)) : decimal.Subtract(seconds, decimal.Divide(nanos, 1000000000m));
    }
    /// <summary>
    /// Constructor taking a .net DateTime object
    /// </summary>
    /// <param name="dateTime">
    /// The DateTime representing the moment in time this
    /// consensus timestamp will represent.
    /// </param>
    public ConsensusTimeStamp(DateTime dateTime)
    {
        Seconds = (decimal)(dateTime - DateTime.UnixEpoch).TotalSeconds;
    }
    /// <summary>
    /// Produces a string representing the consensus time
    /// stamp in the HAPI string form 00000.000000000.
    /// </summary>
    /// <returns>
    /// HAPI string form of the date.
    /// </returns>
    public override string ToString()
    {
        return Seconds.ToString("0.000000000");
    }
    /// <summary>
    /// Compares the current consensus time instance with another
    /// conensus time instance.
    /// </summary>
    /// <param name="other">
    /// The other consensus time stamp to compare against.
    /// </param>
    /// <returns>
    /// Less than zero if this instance represents an earlier
    /// time than the other timestamp, zero of the same and
    /// greater than zero if it is later.
    /// </returns>
    public int CompareTo(ConsensusTimeStamp other)
    {
        return Seconds.CompareTo(other.Seconds);
    }
    /// <summary>
    /// Compares the current consensus time instance with another
    /// object that may be conensus time instance.
    /// </summary>
    /// <param name="obj">
    /// The other object compare against.
    /// </param>
    /// <returns>
    /// Less than zero if this instance represents an earlier
    /// time than the other timestamp, zero of the same and
    /// greater than zero if it is later.
    /// </returns>
    public int CompareTo(object? obj)
    {
        if (obj is ConsensusTimeStamp other)
        {
            return Seconds.CompareTo(other.Seconds);
        }
        return 1;
    }
    /// <summary>
    /// A consensus time stamp representing the current time.
    /// </summary>
    public static ConsensusTimeStamp Now
    {
        get
        {
            return new ConsensusTimeStamp((decimal)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
        }
    }
    /// <summary>
    /// Computes the difference between two consensus timestamps, producing
    /// the decimal fraction of total seconds difference between the two.
    /// </summary>
    /// <param name="lhs">
    /// The Consensus Time Stamp Left hand side of the operator.
    /// </param>
    /// <param name="rhs">
    /// The Consensus Time Stamp Right hand side of the operator.
    /// </param>
    /// <returns>
    /// The fractional number of seconds difference between the two
    /// consensus time stamps.
    /// </returns>
    public static decimal operator -(ConsensusTimeStamp lhs, ConsensusTimeStamp rhs) => lhs.Seconds - rhs.Seconds;
    /// <summary>
    /// Compares the two consensus time stamps, returns true if the
    /// lhs value is earlier in time than the rhs value.
    /// </summary>
    /// <param name="lhs">
    /// The Consensus Time Stamp Left hand side of the operator.
    /// </param>
    /// <param name="rhs">
    /// The Consensus Time Stamp Right hand side of the operator.
    /// </param>
    /// <returns>
    /// True if the lhs value represnts an earlier
    /// time than the rhs value.
    /// </returns>
    public static bool operator <(ConsensusTimeStamp lhs, ConsensusTimeStamp rhs) => lhs.Seconds < rhs.Seconds;
    /// <summary>
    /// Compares the two consensus time stamps, returns true if the
    /// lhs value is equal to or earlier in time than the rhs value.
    /// </summary>
    /// <param name="lhs">
    /// The Consensus Time Stamp Left hand side of the operator.
    /// </param>
    /// <param name="rhs">
    /// The Consensus Time Stamp Right hand side of the operator.
    /// </param>
    /// <returns>
    /// True if the lhs value represents an equal or earlier
    /// time than the rhs value.
    /// </returns>
    public static bool operator <=(ConsensusTimeStamp lhs, ConsensusTimeStamp rhs) => lhs.Seconds <= rhs.Seconds;
    /// <summary>
    /// Compares the two consensus time stamps, returns true if the
    /// lhs value comes after the rhs value.
    /// </summary>
    /// <param name="lhs">
    /// The Consensus Time Stamp Left hand side of the operator.
    /// </param>
    /// <param name="rhs">
    /// The Consensus Time Stamp Right hand side of the operator.
    /// </param>
    /// <returns>
    /// True if if the lhs value comes after the rhs value,
    /// otherwise false.
    /// </returns>
    public static bool operator >(ConsensusTimeStamp lhs, ConsensusTimeStamp rhs) => lhs.Seconds > rhs.Seconds;
    /// <summary>
    /// Compares the two consensus time stamps, returns true if the lhs 
    /// value is equal to or comes after in time than the rhs value.
    /// </summary>
    /// <param name="lhs">
    /// The Consensus Time Stamp Left hand side of the operator.
    /// </param>
    /// <param name="rhs">
    /// The Consensus Time Stamp Right hand side of the operator.
    /// </param>
    /// <returns>
    /// True if if the lhs value equals or comes after the rhs 
    /// value, otherwise false.
    /// </returns>
    public static bool operator >=(ConsensusTimeStamp t1, ConsensusTimeStamp t2) => t1.Seconds >= t2.Seconds;
    /// <summary>
    /// Implicit helper cast to convert an existing .net DateTime
    /// object into a ConsensusTimeStamp
    /// </summary>
    /// <param name="dateTime">
    /// The DateTime object representing the time.
    /// </param>
    public static implicit operator ConsensusTimeStamp(DateTime dateTime)
    {
        return new ConsensusTimeStamp(dateTime);
    }
}