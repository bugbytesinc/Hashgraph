
namespace Hashgraph.Tests;

public class ConsensusTimeStampTests
{
    [Fact(DisplayName = "ConsenusTimeStamp: Equivalent TimeStamps are considered Equal")]
    public void EquivalentTimeStampsAreConsideredEqual()
    {
        var now = DateTime.UtcNow;
        var ts1 = new ConsensusTimeStamp(now);
        var ts2 = new ConsensusTimeStamp(now);
        Assert.Equal(ts1, ts2);
        Assert.True(ts1 == ts2);
        Assert.False(ts1 != ts2);
        Assert.True(ts1 >= ts2);
        Assert.True(ts1 <= ts2);
        Assert.True(ts1.Equals(ts2));
        Assert.True(ts2.Equals(ts1));
    }
    [Fact(DisplayName = "ConsenusTimeStamp: Disimilar TimeStamps are not considered Equal")]
    public void DisimilarTimeStampsAreNotConsideredEqual()
    {
        var ts1 = new ConsensusTimeStamp(DateTime.UtcNow);
        var ts2 = new ConsensusTimeStamp(DateTime.UtcNow.AddSeconds(1));
        Assert.NotEqual(ts1, ts2);
        Assert.False(ts1 == ts2);
        Assert.True(ts1 != ts2);
        Assert.False(ts1 >= ts2);
        Assert.True(ts1 <= ts2);
        Assert.False(ts1.Equals(ts2));
        Assert.False(ts2.Equals(ts1));
    }
    [Fact(DisplayName = "ConsenusTimeStamp: Comparing with other objects are not considered equal.")]
    public void OtherObjectsAreNotConsideredEqualToAddress()
    {
        var ts = new ConsensusTimeStamp(DateTime.UtcNow);
        Assert.False(ts.Equals("Something that is not an address"));
    }
    [Fact(DisplayName = "ConsenusTimeStamp: Consensus Time Stamp cast as object still considered equivalent.")]
    public void ConsensusTimeStampCastAsObjectIsconsideredEqual()
    {
        var ts = new ConsensusTimeStamp(DateTime.UtcNow);
        object equivalent = new ConsensusTimeStamp(ts.Seconds);
        Assert.True(ts.Equals(equivalent));
        Assert.True(equivalent.Equals(ts));
    }
    [Fact(DisplayName = "ConsenusTimeStamp: Consensus Time Stamps as objects but reference equal are same.")]
    public void ConsensusTimeStampsReferenceEqualIsconsideredEqual()
    {
        var ts = new ConsensusTimeStamp(DateTime.UtcNow);
        object reference = ts;
        Assert.True(ts.Equals(ts));
        Assert.True(reference.Equals(ts));
    }
    [Fact(DisplayName = "ConsenusTimeStamp: Can Compute Difference in Seconds")]
    public void CanComputeDifferenceInSeconds()
    {
        var diff = 100m;
        var ts1 = new ConsensusTimeStamp(DateTime.UtcNow);
        var ts2 = new ConsensusTimeStamp(ts1.Seconds + diff);
        Assert.True(ts2 > ts1);
        Assert.Equal(diff, ts2 - ts1);
    }
    [Fact(DisplayName = "ConsenusTimeStamp: Can Construct with nanoseconds")]
    public void CanConstructWithNanoseconds()
    {
        var seconds = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        var nanos = 99;
        var value = decimal.Add(seconds, decimal.Divide(nanos, 1000000000m));
        var ts = new ConsensusTimeStamp(seconds, nanos);
        Assert.Equal(value, ts.Seconds);
    }
    [Fact(DisplayName = "ConsenusTimeStamp: To String includes decimals")]
    public void ToStringIncludesFraction()
    {
        var seconds = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        var nanos = 99;
        var value = decimal.Add(seconds, decimal.Divide(nanos, 1000000000m));
        Assert.Equal($"{seconds}.000000099", new ConsensusTimeStamp(seconds, nanos).ToString());
        Assert.Equal($"{seconds}.000000099", new ConsensusTimeStamp(value).ToString());
        Assert.Equal("10.000000000", new ConsensusTimeStamp(10, 0).ToString());
        Assert.Equal("1000.100000000", new ConsensusTimeStamp(1000.1m).ToString());
        Assert.Equal("-1000.100000000", new ConsensusTimeStamp(-1000.1m).ToString());
        Assert.Equal("0.100000000", new ConsensusTimeStamp(0, 100000000).ToString());
    }
    [Fact(DisplayName = "ConsenusTimeStamp: Can Implictly Case DateTime to ConsensusTimeStamp")]
    public void CanImplictlyCaseDateTimeToConsensusTimeStamp()
    {
        var now = DateTime.UtcNow;
        var ts1 = new ConsensusTimeStamp(now);
        ConsensusTimeStamp ts2 = now;
        Assert.Equal(ts1, ts2);
        Assert.Equal(ts1, now);
        Assert.Equal(ts2, now);
        Assert.True(ts1 == now);
        Assert.True(ts2 == now);
        Assert.False(ts2 != now);
        Assert.False(ts2 < now);
        Assert.False(ts2 > now);
        Assert.True(ts2 <= now);
        Assert.True(ts2 >= now);
    }
}
