using Hashgraph.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures
{
    public static class AssertHg
    {
        public static async Task TokenStatusAsync(TestToken fxToken, TestAccount fxAccount, TokenKycStatus status)
        {
            var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
            Assert.NotNull(info);

            var tokenRecord = info.Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.NotNull(tokenRecord);

            Assert.Equal(status, tokenRecord.KycStatus);
        }

        public static async Task TokenStatusAsync(TestToken fxToken, TestAccount fxAccount, TokenTradableStatus status)
        {
            var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
            Assert.NotNull(info);

            var tokenRecord = info.Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.NotNull(tokenRecord);

            Assert.Equal(status, tokenRecord.TradableStatus);
        }

        public static async Task TokenBalanceAsync(TestToken fxToken, TestAccount fxAccount, ulong expectedBalance)
        {
            var balance = await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken);
            Assert.Equal(expectedBalance, balance);
        }

        internal static async Task TokenNotAssociatedAsync(TestToken fxToken, TestAccount fxAccount)
        {
            var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
            Assert.NotNull(info);

            var association = info.Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.Null(association);
        }

        internal static async Task<TokenBalance> TokenIsAssociatedAsync(TestToken fxToken, TestAccount fxAccount)
        {
            var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
            Assert.NotNull(info);

            var association = info.Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.NotNull(association);

            return association;
        }
        public static Task CryptoBalanceAsync(TestAccount fxAccount, int expectedBalance)
        {
            return CryptoBalanceAsync(fxAccount, (ulong)expectedBalance);
        }
        public static async Task CryptoBalanceAsync(TestAccount fxAccount, ulong expectedBalance)
        {
            var balance = await fxAccount.Client.GetAccountBalanceAsync(fxAccount);
            Assert.Equal(expectedBalance, balance);
        }
    }
}
