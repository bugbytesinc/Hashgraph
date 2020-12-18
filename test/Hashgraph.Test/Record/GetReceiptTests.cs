using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Record
{
    [Collection(nameof(NetworkCredentials))]
    public class GetReceiptTests
    {
        private readonly NetworkCredentials _network;
        public GetReceiptTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Get Receipt: Can get Create Topic Receipt")]
        public async Task CanGetCreateTopicReceipt()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var receipt = await fx.Client.GetReceiptAsync(fx.Record.Id);
            var topicReceipt = Assert.IsType<CreateTopicReceipt>(receipt);
            Assert.Equal(fx.Record.Id, topicReceipt.Id);
            Assert.Equal(fx.Record.Status, topicReceipt.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, topicReceipt.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, topicReceipt.NextExchangeRate);
            Assert.Equal(fx.Record.Topic, topicReceipt.Topic);
        }
        [Fact(DisplayName = "Get Receipt: Can get Submit Message Receipt")]
        public async Task CanGetSubmitMessageReceipt()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
            var receipt1 = await fx.Client.SubmitMessageAsync(fx.Record.Topic, message, fx.ParticipantPrivateKey);
            var receipt2 = await fx.Client.GetReceiptAsync(receipt1.Id);
            var sendReceipt = Assert.IsType<SubmitMessageReceipt>(receipt2);
            Assert.Equal(receipt1.Id, sendReceipt.Id);
            Assert.Equal(receipt1.Status, sendReceipt.Status);
            Assert.Equal(receipt1.CurrentExchangeRate, sendReceipt.CurrentExchangeRate);
            Assert.Equal(receipt1.NextExchangeRate, sendReceipt.NextExchangeRate);
            Assert.Equal(receipt1.SequenceNumber, sendReceipt.SequenceNumber);
            Assert.Equal(receipt1.RunningHash.ToArray(), sendReceipt.RunningHash.ToArray());
            Assert.Equal(receipt1.RunningHashVersion, sendReceipt.RunningHashVersion);
        }
        [Fact(DisplayName = "Get Receipt: Can get Create Contract Receipt")]
        public async Task CanGetCreateContractRecipt()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);
            var receipt = await fx.Client.GetReceiptAsync(fx.ContractRecord.Id);
            var createRecipt = Assert.IsType<CreateContractReceipt>(receipt);

            Assert.Equal(fx.ContractRecord.Id, createRecipt.Id);
            Assert.Equal(fx.ContractRecord.Status, createRecipt.Status);
            Assert.Equal(fx.ContractRecord.CurrentExchangeRate, createRecipt.CurrentExchangeRate);
            Assert.Equal(fx.ContractRecord.NextExchangeRate, createRecipt.NextExchangeRate);
            Assert.Equal(fx.ContractRecord.Contract, createRecipt.Contract);
        }
        [Fact(DisplayName = "Get Receipt: Can get Create Account Receipt")]
        public async Task CanGetCreateAccountReceipt()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var receipt = await fx.Client.GetReceiptAsync(fx.Record.Id);
            var accountReceipt = Assert.IsType<CreateAccountReceipt>(receipt);
            Assert.Equal(fx.Record.Id, accountReceipt.Id);
            Assert.Equal(fx.Record.Status, accountReceipt.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, accountReceipt.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, accountReceipt.NextExchangeRate);
            Assert.Equal(fx.Record.Address, accountReceipt.Address);
        }
        [Fact(DisplayName = "Get Receipt: Can get Create File Receipt")]
        public async Task CanGetCreateFileReceipt()
        {
            await using var fx = await TestFile.CreateAsync(_network);
            var receipt = await fx.Client.GetReceiptAsync(fx.Record.Id);
            var fileReceipt = Assert.IsType<FileReceipt>(receipt);
            Assert.Equal(fx.Record.Id, fileReceipt.Id);
            Assert.Equal(fx.Record.Status, fileReceipt.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, fileReceipt.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, fileReceipt.NextExchangeRate);
            Assert.Equal(fx.Record.File, fileReceipt.File);
        }
        [Fact(DisplayName = "Get Receipt: Can get Create Token Receipt")]
        public async Task CanGetCreateTokenReceipt()
        {
            await using var fx = await TestToken.CreateAsync(_network);
            var receipt = await fx.Client.GetReceiptAsync(fx.Record.Id);
            var createReceipt = Assert.IsType<CreateTokenReceipt>(receipt);
            Assert.Equal(fx.Record.Id, createReceipt.Id);
            Assert.Equal(fx.Record.Status, createReceipt.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, createReceipt.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, createReceipt.NextExchangeRate);
            Assert.Equal(fx.Record.Token, createReceipt.Token);
        }
        [Fact(DisplayName = "Get Receipt: Can get List of Duplicate Receipts")]
        public async Task CanGetListOfDuplicateReceipts()
        {
            for (int tries = 0; tries < 5; tries++)
            {
                var duplicates = Generator.Integer(10, 15);
                var passedPrecheck = duplicates;
                await using var client = _network.NewClient();
                var txid = client.CreateNewTxId();
                var tasks = new Task[duplicates];
                for (var i = 0; i < duplicates; i++)
                {
                    tasks[i] = client.TransferAsync(_network.Payer, _network.Gateway, 1, ctx => ctx.Transaction = txid);
                }
                for (var i = 0; i < duplicates; i++)
                {
                    try
                    {
                        await tasks[i];
                    }
                    catch
                    {
                        passedPrecheck--;
                    }
                }
                if (passedPrecheck == 0)
                {
                    // Start over.
                    continue;
                }
                var receipts = await client.GetAllReceiptsAsync(txid);
                Assert.Equal(passedPrecheck, receipts.Count);
                Assert.Equal(1, receipts.Count(t => t.Status == ResponseCode.Success));
                Assert.Equal(passedPrecheck - 1, receipts.Count(t => t.Status == ResponseCode.DuplicateTransaction));
                return;
            }
            _network.Output?.WriteLine("TEST INCONCLUSIVE, UNABLE TO CREATE DUPLICATE TRANSACTIONS THIS TIME AROUND.");
        }
        [Fact(DisplayName = "Get Receipt: Can get List of One Receipt")]
        public async Task CanGetListOfOneReceipt()
        {
            await using var client = _network.NewClient();
            var receipt = await client.TransferAsync(_network.Payer, _network.Gateway, 1);
            var receipts = await client.GetAllReceiptsAsync(receipt.Id);
            Assert.Single(receipts);
            Assert.Equal(ResponseCode.Success, receipts[0].Status);
        }
        [Fact(DisplayName = "Get Receipt: Can get List of No Receipts")]
        public async Task CanGetListOfNoReceipts()
        {
            await using var client = _network.NewClient();
            var txid = client.CreateNewTxId();
            var receipts = await client.GetAllReceiptsAsync(txid);
            Assert.Empty(receipts);
        }
        [Fact(DisplayName = "Get Receipt: Can get Create Token Receipt as List")]
        public async Task CanGetCreateTokenReceiptAsList()
        {
            await using var fx = await TestToken.CreateAsync(_network);
            var receipts = await fx.Client.GetAllReceiptsAsync(fx.Record.Id);
            Assert.Single(receipts);
            var receipt = receipts[0];
            var createReceipt = Assert.IsType<CreateTokenReceipt>(receipt);
            Assert.Equal(fx.Record.Id, createReceipt.Id);
            Assert.Equal(fx.Record.Status, createReceipt.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, createReceipt.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, createReceipt.NextExchangeRate);
            Assert.Equal(fx.Record.Token, createReceipt.Token);
        }
        [Fact(DisplayName = "Get Receipt: Can get a TookenReceipt for Burn")]
        public async Task CanGetTokenReceiptForBurn()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var amountToDestory = fxToken.Params.Circulation / 3;
            var expectedCirculation = fxToken.Params.Circulation - amountToDestory;

            var originalReceipt = await fxToken.Client.BurnTokenAsync(fxToken, amountToDestory, fxToken.SupplyPrivateKey);
            Assert.Equal(ResponseCode.Success, originalReceipt.Status);
            Assert.Equal(expectedCirculation, originalReceipt.Circulation);

            var copyReceipt = await fxToken.Client.GetReceiptAsync(originalReceipt.Id);
            var tokenRecipt = Assert.IsType<TokenReceipt>(copyReceipt);
            Assert.Equal(originalReceipt.Id, tokenRecipt.Id);
            Assert.Equal(originalReceipt.Status, tokenRecipt.Status);
            Assert.Equal(originalReceipt.CurrentExchangeRate, tokenRecipt.CurrentExchangeRate);
            Assert.Equal(originalReceipt.NextExchangeRate, tokenRecipt.NextExchangeRate);
            Assert.Equal(originalReceipt.Circulation, tokenRecipt.Circulation);
        }
        [Fact(DisplayName = "Get Receipt: Can get a TookenReceipt for Confiscate")]
        public async Task CanGetTokenReceiptForConfiscate()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
            var xferAmount = 2 * fxToken.Params.Circulation / (ulong)Generator.Integer(3, 5);
            var expectedTreasury = fxToken.Params.Circulation - xferAmount;

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            Assert.Equal(xferAmount, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken));
            Assert.Equal(expectedTreasury, await fxAccount.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

            var originalReceipt = await fxToken.Client.ConfiscateTokensAsync(fxToken, fxAccount, xferAmount, fxToken.ConfiscatePrivateKey);
            Assert.Equal(ResponseCode.Success, originalReceipt.Status);
            Assert.Equal(expectedTreasury, originalReceipt.Circulation);

            var copyReceipt = await fxToken.Client.GetReceiptAsync(originalReceipt.Id);
            var tokenReceipt = Assert.IsType<TokenReceipt>(copyReceipt);
            Assert.Equal(originalReceipt.Id, tokenReceipt.Id);
            Assert.Equal(originalReceipt.Status, tokenReceipt.Status);
            Assert.Equal(originalReceipt.CurrentExchangeRate, tokenReceipt.CurrentExchangeRate);
            Assert.Equal(originalReceipt.NextExchangeRate, tokenReceipt.NextExchangeRate);
            Assert.Equal(originalReceipt.Circulation, tokenReceipt.Circulation);
        }
        [Fact(DisplayName = "Get Receipt: Can get a TookenReceipt for Mint")]
        public async Task CanGetTokenReceiptForMint()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var originalReceipt = await fxToken.Client.MintTokenAsync(fxToken.Record.Token, fxToken.Params.Circulation, fxToken.SupplyPrivateKey);
            Assert.Equal(ResponseCode.Success, originalReceipt.Status);
            Assert.Equal(fxToken.Params.Circulation * 2, originalReceipt.Circulation);

            var copyReceipt = await fxToken.Client.GetReceiptAsync(originalReceipt.Id);
            var tokenReceipt = Assert.IsType<TokenReceipt>(copyReceipt);
            Assert.Equal(originalReceipt.Id, tokenReceipt.Id);
            Assert.Equal(originalReceipt.Status, tokenReceipt.Status);
            Assert.Equal(originalReceipt.CurrentExchangeRate, tokenReceipt.CurrentExchangeRate);
            Assert.Equal(originalReceipt.NextExchangeRate, tokenReceipt.NextExchangeRate);
            Assert.Equal(originalReceipt.Circulation, tokenReceipt.Circulation);
        }
    }
}
