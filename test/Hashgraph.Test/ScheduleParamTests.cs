using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests
{
    public class ScheduleParamsTests
    {
        [Fact(DisplayName = "Schedule Params: Can Create Valid PendingParams Object")]
        public void CreateValidPendingParamsObject()
        {
            var pending = new ScheduleParams();
            Assert.Null(pending.PendingPayer);
            Assert.Null(pending.Administrator);
            Assert.Null(pending.Memo);
            Assert.Null(pending.Signatory);
            Assert.Equal(pending, new ScheduleParams());
        }
        [Fact(DisplayName = "Schedule Params: Equivalent PendingParams are considered Equal")]
        public void EquivalentPendingParamsAreConsideredEqual()
        {
            var payer = new Address(Generator.Integer(1, 5), Generator.Integer(1, 5), Generator.Integer(1, 5));
            var (adminKey, signatoryKey) = Generator.KeyPair();
            var memo = Generator.String(5, 75);
            var pendingParam1 = new ScheduleParams
            {
                PendingPayer = payer,
                Administrator = adminKey,
                Memo = memo,
                Signatory = signatoryKey
            };
            var pendingParam2 = new ScheduleParams
            {
                PendingPayer = payer,
                Administrator = adminKey,
                Memo = memo,
                Signatory = signatoryKey
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
            var (adminKey, signatoryKey) = Generator.KeyPair();
            var memo = Generator.String(5, 75);
            var pendingParams1 = new ScheduleParams
            {
                PendingPayer = payer,
                Administrator = adminKey,
                Memo = memo,
                Signatory = signatoryKey
            };

            var pendingParams2 = new ScheduleParams
            {
                Administrator = adminKey,
                Memo = memo,
                Signatory = signatoryKey
            };
            Assert.NotEqual(pendingParams1, pendingParams2);
            Assert.False(pendingParams1 == pendingParams2);
            Assert.True(pendingParams1 != pendingParams2);

            pendingParams2 = new ScheduleParams
            {
                PendingPayer = payer,
                Memo = memo,
                Signatory = signatoryKey
            };
            Assert.NotEqual(pendingParams1, pendingParams2);
            Assert.False(pendingParams1 == pendingParams2);
            Assert.True(pendingParams1 != pendingParams2);

            pendingParams2 = new ScheduleParams
            {
                PendingPayer = payer,
                Administrator = adminKey,
                Signatory = signatoryKey
            };
            Assert.NotEqual(pendingParams1, pendingParams2);
            Assert.False(pendingParams1 == pendingParams2);
            Assert.True(pendingParams1 != pendingParams2);

            pendingParams2 = new ScheduleParams
            {
                PendingPayer = payer,
                Administrator = adminKey,
                Memo = memo,
            };
            Assert.NotEqual(pendingParams1, pendingParams2);
            Assert.False(pendingParams1 == pendingParams2);
            Assert.True(pendingParams1 != pendingParams2);

            pendingParams2 = new ScheduleParams
            {
                PendingPayer = payer,
                Administrator = adminKey,
                Memo = Generator.Code(80),
                Signatory = signatoryKey
            };
            Assert.NotEqual(pendingParams1, pendingParams2);
            Assert.False(pendingParams1 == pendingParams2);
            Assert.True(pendingParams1 != pendingParams2);
        }
    }
}
