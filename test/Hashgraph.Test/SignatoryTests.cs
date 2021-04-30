using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Tests
{
    public class SignatoriesTests
    {
        [Fact(DisplayName = "Signatories: Can Create Valid Signatories Object")]
        public void CreateValidSignatoriesObject()
        {
            var (_, privateKey1) = Generator.KeyPair();
            var (_, privateKey2) = Generator.KeyPair();

            new Signatory(privateKey1);
            new Signatory(privateKey1, privateKey2);
            new Signatory(new Signatory(privateKey1, privateKey2), new Signatory(privateKey1, privateKey2));
        }
        [Fact(DisplayName = "Signatories: Empty Private key throws Exception")]
        public void EmptyValueForKeyThrowsError()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Signatory();
            });
            Assert.Equal("signatories", exception.ParamName);
            Assert.StartsWith("At least one Signatory in a list is required.", exception.Message);
        }
        [Fact(DisplayName = "Signatories: Invalid Bytes in Private key throws Exception")]
        public void InvalidBytesForValueForKeyThrowsError()
        {
            var (_, originalKey) = Generator.KeyPair();
            var invalidKey = originalKey.ToArray();
            invalidKey[0] = 0;
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Signatory(KeyType.Ed25519, invalidKey);
            });
            Assert.StartsWith("The private key was not provided in a recognizable Ed25519 format.", exception.Message);
        }
        [Fact(DisplayName = "Signatories: Invalid Byte Length in Private key throws Exception")]
        public void InvalidByteLengthForValueForKeyThrowsError()
        {
            var (_, originalKey) = Generator.KeyPair();
            var invalidKey = originalKey.ToArray().Take(32).ToArray();
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Signatory(KeyType.Ed25519, invalidKey);
            });
            Assert.StartsWith("The private key was not provided in a recognizable Ed25519 format.", exception.Message);
        }
        [Fact(DisplayName = "Signatories: Equivalent Signatories are considered Equal")]
        public void EquivalentSignatoriesAreConsideredEqual()
        {
            var (_, privateKey1) = Generator.KeyPair();
            var (_, privateKey2) = Generator.KeyPair();
            var signatory1 = new Signatory(privateKey1);
            var signatory2 = new Signatory(privateKey1);
            Assert.Equal(signatory1, signatory2);
            Assert.True(signatory1 == signatory2);
            Assert.False(signatory1 != signatory2);

            signatory1 = new Signatory(privateKey1, privateKey2);
            signatory2 = new Signatory(privateKey1, privateKey2);
            Assert.Equal(signatory1, signatory2);
            Assert.True(signatory1 == signatory2);
            Assert.False(signatory1 != signatory2);
        }
        [Fact(DisplayName = "Signatories: Disimilar Signatories are not considered Equal")]
        public void DisimilarSignatoriesAreNotConsideredEqual()
        {
            var (_, privateKey1) = Generator.KeyPair();
            var (_, privateKey2) = Generator.KeyPair();
            var signatory1 = new Signatory(privateKey1);
            var signatory2 = new Signatory(privateKey2);
            Assert.NotEqual(signatory1, signatory2);
            Assert.False(signatory1 == signatory2);
            Assert.True(signatory1 != signatory2);

            signatory1 = new Signatory(privateKey1);
            signatory2 = new Signatory(privateKey1, privateKey2);
            Assert.NotEqual(signatory1, signatory2);
            Assert.False(signatory1 == signatory2);
            Assert.True(signatory1 != signatory2);

            signatory1 = new Signatory(privateKey1, privateKey2);
            signatory2 = new Signatory(privateKey2, privateKey1);
            Assert.NotEqual(signatory1, signatory2);
            Assert.False(signatory1 == signatory2);
            Assert.True(signatory1 != signatory2);
        }

        [Fact(DisplayName = "Signatories: Disimilar Multi-Key Signatories are not considered Equal")]
        public void DisimilarMultiKeySignatoriesAreNotConsideredEqual()
        {
            var (_, privateKey1) = Generator.KeyPair();
            var (_, privateKey2) = Generator.KeyPair();
            var (_, privateKey3) = Generator.KeyPair();
            var signatories1 = new Signatory(privateKey1, privateKey2);
            var signatories2 = new Signatory(privateKey2, privateKey3);
            Assert.NotEqual(signatories1, signatories2);
            Assert.False(signatories1 == signatories2);
            Assert.True(signatories1 != signatories2);

            signatories1 = new Signatory(privateKey1, privateKey2, privateKey3);
            signatories2 = new Signatory(privateKey1, privateKey2);
            Assert.NotEqual(signatories1, signatories2);
            Assert.False(signatories1 == signatories2);
            Assert.True(signatories1 != signatories2);

            signatories1 = new Signatory(privateKey2, privateKey3, privateKey1);
            signatories2 = new Signatory(privateKey1, privateKey2, privateKey3);
            Assert.NotEqual(signatories1, signatories2);
            Assert.False(signatories1 == signatories2);
            Assert.True(signatories1 != signatories2);
        }
        [Fact(DisplayName = "Signatories: Equivalent Complex Signatories are considered Equal")]
        public void EquivalentComplexSignatoriesAreConsideredEqual()
        {
            Func<IInvoice, Task> callback = ctx => { return Task.FromResult(0); };
            var (_, privateKey1) = Generator.KeyPair();
            var (_, privateKey2) = Generator.KeyPair();
            var (_, privateKey3) = Generator.KeyPair();
            var signatory1 = new Signatory(callback);
            var signatory2 = new Signatory(callback);
            Assert.Equal(signatory1, signatory2);
            Assert.True(signatory1 == signatory2);
            Assert.False(signatory1 != signatory2);

            signatory1 = new Signatory(privateKey1, new Signatory(callback));
            signatory2 = new Signatory(privateKey1, callback);
            Assert.Equal(signatory1, signatory2);
            Assert.True(signatory1 == signatory2);
            Assert.False(signatory1 != signatory2);
            signatory1 = new Signatory(privateKey1, callback, new Signatory(privateKey2, privateKey3));
            signatory2 = new Signatory(privateKey1, callback, new Signatory(privateKey2, privateKey3));
            Assert.Equal(signatory1, signatory2);
            Assert.True(signatory1 == signatory2);
            Assert.False(signatory1 != signatory2);
        }
        [Fact(DisplayName = "Signatories: Only Reference Equal Callbacks are considered Equal")]
        public void CallbackSignatoriesAreOnlyReferenceEqual()
        {
            static Task callback1(IInvoice ctx) { return Task.FromResult(0); }
            static Task callback2(IInvoice ctx) { return Task.FromResult(0); }

            var signatory1 = new Signatory(callback1);
            var signatory2 = new Signatory(callback2);
            Assert.NotEqual(signatory1, signatory2);
            Assert.False(signatory1 == signatory2);
            Assert.True(signatory1 != signatory2);
        }
        [Fact(DisplayName = "Signatories: Null PendingParams raises an Error")]
        public void NullPendingParamsRaisesAnError()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new Signatory((PendingParams)null);
            });
            Assert.Equal("pendingParams", exception.ParamName);
            Assert.StartsWith("Pending Parameters object cannot be null.", exception.Message);
        }
        [Fact(DisplayName = "Signatories: Empty Scheduled Signatories Are Considered Equal")]
        public void EmptyScheduledSignatoriesAreConsideredEqual()
        {
            var schedule1 = new PendingParams();
            var schedule2 = new PendingParams();

            var signatory1 = new Signatory(schedule1);
            var signatory2 = new Signatory(schedule2);
            Assert.Equal(signatory1, signatory2);
            Assert.True(signatory1 == signatory2);
            Assert.False(signatory1 != signatory2);
        }
        [Fact(DisplayName = "Signatories: Dissimilar Signatory Schedules Are Considered Not Equal")]
        public void DissimilarSignatorySchedulesAreConsideredNotEqual()
        {
            var schedule1 = new PendingParams
            {
                Memo = "memo 1"
            };
            var schedule2 = new PendingParams
            {
                Memo = "Memo 2"
            };

            var signatory1 = new Signatory(schedule1);
            var signatory2 = new Signatory(schedule2);
            Assert.NotEqual(signatory1, signatory2);
            Assert.False(signatory1 == signatory2);
            Assert.True(signatory1 != signatory2);
        }
        [Fact(DisplayName = "Signatories: Similar Signatory Schedules Are Considered Not Equal")]
        public void SimilarSignatorySchedulesAreConsideredNotEqual()
        {
            var schedule1 = new PendingParams
            {
                Memo = "memo 1"
            };
            var schedule2 = new PendingParams
            {
                Memo = "memo 1"
            };

            var signatory1 = new Signatory(schedule1);
            var signatory2 = new Signatory(schedule2);
            Assert.Equal(signatory1, signatory2);
            Assert.True(signatory1 == signatory2);
            Assert.False(signatory1 != signatory2);
        }
        [Fact(DisplayName = "Signatories: Can Retrieve Schedule Params From Signatory")]
        public void CanRetrieveScheduleParamsFromSignatory()
        {
            var schedule = new PendingParams
            {
                Memo = Generator.Code(20)
            };
            var signatory = new Signatory(schedule);
            var retrieved = ((ISignatory)signatory).GetSchedule();
            Assert.Equal(schedule, retrieved);
        }
        [Fact(DisplayName = "Signatories: Can Retrieve Nested Schedule Params From Signatory")]
        public void CanRetrieveNestedScheduleParamsFromSignatory()
        {
            var (_, randomKey) = Generator.KeyPair();
            var schedule = new PendingParams
            {
                Memo = Generator.Code(20)
            };
            var signatory = new Signatory(new Signatory(new Signatory(new Signatory(schedule))), randomKey);
            var retrieved = ((ISignatory)signatory).GetSchedule();
            Assert.Equal(schedule, retrieved);
        }
        [Fact(DisplayName = "Signatories: Multiple Schedules Accepted when Identical")]
        public void MultipleSchedulesAcceptedWhenIdentical()
        {
            var memo = Generator.Code(50);
            var (_, randomKey) = Generator.KeyPair();
            var schedule1 = new PendingParams { Memo = memo };
            var schedule2 = new PendingParams { Memo = memo };
            var signatory = new Signatory(new Signatory(new Signatory(new Signatory(schedule1)), schedule2), randomKey);
            var retrieved = ((ISignatory)signatory).GetSchedule();
            Assert.Equal(schedule1, retrieved);
            Assert.Equal(schedule1, schedule2);
        }
        [Fact(DisplayName = "Signatories: Multiple Dissimilar Schedules Raises an Error On Retrieval")]
        public void MultipleDissimilarSchedulesRaisesAnErrorOnRetrieval()
        {
            var (_, randomKey) = Generator.KeyPair();
            var schedule1 = new PendingParams { Memo = Generator.Code(50) };
            var schedule2 = new PendingParams { Memo = Generator.Code(50) };
            var signatory = new Signatory(new Signatory(new Signatory(new Signatory(schedule1)), schedule2), randomKey);
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                ((ISignatory)signatory).GetSchedule();
            });
            Assert.Equal("Found Multiple Pending Signatories, do not know which one to choose.", exception.Message);
        }
    }
}
