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
            Func<IInvoice,Task> callback = ctx => { return Task.FromResult(0); };
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
            signatory1 = new Signatory(privateKey1, callback, new Signatory(privateKey2,privateKey3));
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
    }
}
