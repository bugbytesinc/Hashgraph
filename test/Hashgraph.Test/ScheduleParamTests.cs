using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests
{
    public class ScheduleParamsTests
    {
        [Fact(DisplayName = "Schedule Params: Can Create Valid PendingParams Object")]
        public void CreateValidPendingParamsObject()
        {
            var pending = new PendingParams();
            Assert.Null(pending.PendingPayer);
            Assert.Null(pending.Administrator);
            Assert.Null(pending.Memo);
            Assert.Equal(pending, new PendingParams());
        }
        [Fact(DisplayName = "Schedule Params: Equivalent PendingParams are considered Equal")]
        public void EquivalentPendingParamsAreConsideredEqual()
        {
            var payer = new Address(Generator.Integer(1, 5), Generator.Integer(1, 5), Generator.Integer(1, 5));
            var (adminKey, _) = Generator.KeyPair();
            var memo = Generator.String(5, 75);
            var pendingParam1 = new PendingParams
            {
                PendingPayer = payer,
                Administrator = adminKey,
                Memo = memo
            };
            var pendingParam2 = new PendingParams
            {
                PendingPayer = payer,
                Administrator = adminKey,
                Memo = memo
            };

            Assert.Equal(pendingParam1, pendingParam2);
            Assert.True(pendingParam1 == pendingParam2);
            Assert.False(pendingParam1 != pendingParam2);

            object asObject1 = pendingParam1;
            object asObject2 = pendingParam2;
            Assert.Equal(asObject1, asObject2);
            Assert.True(pendingParam1.Equals(asObject1));
            Assert.True(asObject1.Equals(pendingParam1));
            Assert.True(pendingParam1.Equals(asObject2));
            Assert.True(asObject1.Equals(pendingParam2));
        }
        [Fact(DisplayName = "Schedule Params:Disimilar PendingParams are not considered Equal")]
        public void DisimilarPendingParamsAreNotConsideredEqual()
        {
            var payer = new Address(Generator.Integer(1, 5), Generator.Integer(1, 5), Generator.Integer(1, 5));
            var (adminKey, _) = Generator.KeyPair();
            var memo = Generator.String(5, 75);
            var pendingParams1 = new PendingParams
            {
                PendingPayer = payer,
                Administrator = adminKey,
                Memo = memo
            };

            var pendingParams2 = new PendingParams
            {
                Administrator = adminKey,
                Memo = memo
            };
            Assert.NotEqual(pendingParams1, pendingParams2);
            Assert.False(pendingParams1 == pendingParams2);
            Assert.True(pendingParams1 != pendingParams2);

            pendingParams2 = new PendingParams
            {
                PendingPayer = payer,
                Memo = memo
            };
            Assert.NotEqual(pendingParams1, pendingParams2);
            Assert.False(pendingParams1 == pendingParams2);
            Assert.True(pendingParams1 != pendingParams2);

            pendingParams2 = new PendingParams
            {
                PendingPayer = payer,
                Administrator = adminKey
            };
            Assert.NotEqual(pendingParams1, pendingParams2);
            Assert.False(pendingParams1 == pendingParams2);
            Assert.True(pendingParams1 != pendingParams2);

            pendingParams2 = new PendingParams
            {
                PendingPayer = payer,
                Administrator = adminKey,
                Memo = Generator.Code(80)
            };
            Assert.NotEqual(pendingParams1, pendingParams2);
            Assert.False(pendingParams1 == pendingParams2);
            Assert.True(pendingParams1 != pendingParams2);
        }
    }
}
