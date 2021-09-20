using Hashgraph.Extensions;
using System.Collections.ObjectModel;
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

        public static async Task AssetStatusAsync(TestAsset fxAsset, TestAccount fxAccount, TokenKycStatus status)
        {
            var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
            Assert.NotNull(info);

            var tokenRecord = info.Tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
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

        public static async Task AssetStatusAsync(TestAsset fxAsset, TestAccount fxAccount, TokenTradableStatus status)
        {
            var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
            Assert.NotNull(info);

            var tokenRecord = info.Tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
            Assert.NotNull(tokenRecord);

            Assert.Equal(status, tokenRecord.TradableStatus);
        }

        public static async Task TokenBalanceAsync(TestToken fxToken, TestAccount fxAccount, ulong expectedBalance)
        {
            var balance = await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken);
            Assert.Equal(expectedBalance, balance);
        }
        public static async Task AssetBalanceAsync(TestAsset fxAsset, TestAccount fxAccount, ulong expectedBalance)
        {
            var balance = await fxAsset.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset);
            Assert.Equal(expectedBalance, balance);
        }
        public static async Task AssetBalanceAsync(TestAsset fxAsset, TestAccount fxAccount, int expectedBalance)
        {
            var balance = await fxAsset.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset);
            Assert.Equal((ulong)expectedBalance, balance);
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

        internal static async Task AssetNotAssociatedAsync(TestAsset fxAsset, TestAccount fxAccount)
        {
            var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
            Assert.NotNull(info);

            var association = info.Tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
            Assert.Null(association);
        }

        internal static async Task<TokenBalance> AssetIsAssociatedAsync(TestAsset fxAsset, TestAccount fxAccount)
        {
            var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
            Assert.NotNull(info);

            var association = info.Tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
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

        internal static void ContainsRoyalty(TestToken fxToken, TestAccount fxPayer, TestAccount fxReceiver, int amount, ReadOnlyCollection<RoyaltyTransfer> royalties)
        {
            var token = fxToken.Record.Token;
            var payer = fxPayer.Record.Address;
            var receiver = fxReceiver.Record.Address;
            foreach(var entry in royalties)
            {
                if(amount == entry.Amount && token == entry.Token && receiver == entry.Receiver && entry.Payers.Contains(payer))
                {
                    return;
                }
            }
            throw new Xunit.Sdk.XunitException($"Unable to find royalty payment using token {token} involving a payer {payer} to receiver {receiver} with amount {amount}.");
        }
        internal static void ContainsHbarRoyalty(TestAccount fxPayer, TestAccount fxReceiver, int amount, ReadOnlyCollection<RoyaltyTransfer> royalties)
        {
            var payer = fxPayer.Record.Address;
            var receiver = fxReceiver.Record.Address;
            foreach (var entry in royalties)
            {
                if (amount == entry.Amount && Address.None == entry.Token && receiver == entry.Receiver && entry.Payers.Contains(payer))
                {
                    return;
                }
            }
            throw new Xunit.Sdk.XunitException($"Unable to find royalty payment using hBbar involving a payer {payer} to receiver {receiver} with amount {amount}.");
        }
        internal static void SingleAssociation(TestToken fxToken, TestAccount fxAccount, ReadOnlyCollection<Association> associations)
        {
            Assert.NotNull(associations);
            Assert.Single(associations);
            if(fxToken.Record.Token != associations[0].Token || fxAccount.Record.Address != associations[0].Account)
            {
                throw new Xunit.Sdk.XunitException($"Unable to find association record using token {fxToken.Record.Token} with account {fxAccount.Record.Address} .");
            }
        }
        internal static void SingleAssociation(TestAsset fxAsset, TestAccount fxAccount, ReadOnlyCollection<Association> associations)
        {
            Assert.NotNull(associations);
            Assert.Single(associations);
            if (fxAsset.Record.Token != associations[0].Token || fxAccount.Record.Address != associations[0].Account)
            {
                throw new Xunit.Sdk.XunitException($"Unable to find association record using asset {fxAsset.Record.Token} with account {fxAccount.Record.Address} .");
            }
        }
    }
}
