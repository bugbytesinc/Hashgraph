using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using Proto;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Record
{
    [Collection(nameof(NetworkCredentials))]
    public class GetRecordTests
    {
        private readonly NetworkCredentials _network;
        public GetRecordTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Get Record: Can get Transaction Record")]
        public async Task CanGetTransactionRecord()
        {
            await using var fx = await TestAccount.CreateAsync(_network);

            var amount = Generator.Integer(20, 30);
            var receipt = await fx.Client.TransferAsync(_network.Payer, fx.Record.Address, amount);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            var record = await fx.Client.GetTransactionRecordAsync(receipt.Id);
            Assert.NotNull(record);
            Assert.Equal(receipt.Id, record.Id);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
        }
        [Fact(DisplayName = "Get Record: Empty Transaction ID throws error.")]
        public async Task EmptyTransactionIdThrowsError()
        {
            await using var client = _network.NewClient();
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.GetTransactionRecordAsync(null);
            });
            Assert.Equal("transaction", ane.ParamName);
            Assert.StartsWith("Transaction is missing. Please check that it is not null.", ane.Message);
        }

        [Fact(DisplayName = "NETWORK V0.7.0 REGRESSION: Get Record: Invalid Transaction ID throws error.")]
        public async Task InvalidTransactionIdThrowsErrorNetVersion070Regression()
        {
            // The following unit test used to throw a precheck exception because
            // the COST_ANSWER would error out if the record did not exist, will
            // this be restored or is this the new (wasteful) behavior?  For now
            // mark this test as a regression and we will wait and see if it changes
            // in the next version.  If not, we will need to look into changing
            // the behavior of the library in an attempt to not waste client hBars.
            var testFailException = (await Assert.ThrowsAsync<Xunit.Sdk.ThrowsException>(InvalidTransactionIdThrowsError));
            Assert.StartsWith("Assert.Throws() Failure", testFailException.Message);

            // [Fact(DisplayName = "Get Record: Invalid Transaction ID throws error.")]
            async Task InvalidTransactionIdThrowsError()
            {
                await using var client = _network.NewClient();
                var txId = new Proto.TransactionID { AccountID = new Proto.AccountID(_network.Payer), TransactionValidStart = new Proto.Timestamp { Seconds = 500, Nanos = 100 } }.AsTxId();
                var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
                {
                    await client.GetTransactionRecordAsync(txId);
                });
                Assert.Equal(ResponseCode.RecordNotFound, pex.Status);
                Assert.StartsWith("Transaction Failed Pre-Check: RecordNotFound", pex.Message);
            }
        }
        [Fact(DisplayName = "Get Record: Can Get Record for Existing but Failed Transaction")]
        public async Task CanGetRecordForFailedTransaction()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.CallContractWithRecordAsync(new CallContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    Gas = 4000,
                    FunctionName = "not_a_real_method",
                });
            });
            var record = await fx.Client.GetTransactionRecordAsync(tex.TxId);
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.ContractRevertExecuted, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        }
        [Fact(DisplayName = "Get Record: Can get Create Topic Record")]
        public async Task CanGetCreateTopicRecord()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var record = await fx.Client.GetTransactionRecordAsync(fx.Record.Id);
            var topicRecord = Assert.IsType<CreateTopicRecord>(record);
            Assert.Equal(fx.Record.Id, topicRecord.Id);
            Assert.Equal(fx.Record.Status, topicRecord.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, topicRecord.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, topicRecord.NextExchangeRate);
            Assert.Equal(fx.Record.Hash.ToArray(), topicRecord.Hash.ToArray());
            Assert.Equal(fx.Record.Concensus, topicRecord.Concensus);
            Assert.Equal(fx.Record.Memo, topicRecord.Memo);
            Assert.Equal(fx.Record.Fee, topicRecord.Fee);
            Assert.Equal(fx.Record.Topic, topicRecord.Topic);
        }
        [Fact(DisplayName = "Get Record: Can get Submit Message Record")]
        public async Task CanGetSubmitMessageRecord()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
            var record1 = await fx.Client.SubmitMessageWithRecordAsync(fx.Record.Topic, message, fx.ParticipantPrivateKey);
            var record2 = await fx.Client.GetTransactionRecordAsync(record1.Id);
            var submitRecord = Assert.IsType<SubmitMessageRecord>(record2);
            Assert.Equal(record1.Id, submitRecord.Id);
            Assert.Equal(record1.Status, submitRecord.Status);
            Assert.Equal(record1.CurrentExchangeRate, submitRecord.CurrentExchangeRate);
            Assert.Equal(record1.NextExchangeRate, submitRecord.NextExchangeRate);
            Assert.Equal(record1.Hash.ToArray(), submitRecord.Hash.ToArray());
            Assert.Equal(record1.Concensus, submitRecord.Concensus);
            Assert.Equal(record1.Memo, submitRecord.Memo);
            Assert.Equal(record1.Fee, submitRecord.Fee);
            Assert.Equal(record1.SequenceNumber, submitRecord.SequenceNumber);
            Assert.Equal(record1.RunningHash.ToArray(), submitRecord.RunningHash.ToArray());
            Assert.Equal(record1.RunningHashVersion, submitRecord.RunningHashVersion);
        }
        [Fact(DisplayName = "Get Record: Can get Call Contract Record")]
        public async Task CanGetCallContractRecord()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var record1 = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 4000,
                FunctionName = "greet"
            }, ctx => ctx.Memo = "Call Contract");
            var record2 = await fx.Client.GetTransactionRecordAsync(record1.Id);
            var callRecord = Assert.IsType<CallContractRecord>(record2);
            Assert.Equal(record1.Id, callRecord.Id);
            Assert.Equal(record1.Status, callRecord.Status);
            Assert.Equal(record1.CurrentExchangeRate, callRecord.CurrentExchangeRate);
            Assert.Equal(record1.NextExchangeRate, callRecord.NextExchangeRate);
            Assert.Equal(record1.Hash.ToArray(), callRecord.Hash.ToArray());
            Assert.Equal(record1.Concensus, callRecord.Concensus);
            Assert.Equal(record1.Memo, callRecord.Memo);
            Assert.Equal(record1.Fee, callRecord.Fee);
            Assert.Equal(record1.CallResult.Error, callRecord.CallResult.Error);
            Assert.Equal(record1.CallResult.Bloom, callRecord.CallResult.Bloom);
            Assert.Equal(record1.CallResult.Gas, callRecord.CallResult.Gas);
            Assert.Equal(record1.CallResult.Events, callRecord.CallResult.Events);
            Assert.Equal(record1.CallResult.CreatedContracts, callRecord.CallResult.CreatedContracts);
            Assert.Equal(record1.CallResult.Result.As<string>(), callRecord.CallResult.Result.As<string>());
        }
        [Fact(DisplayName = "Get Record: Can get Create Topic Record")]
        public async Task CanGetCreateContractRecord()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var record = await fx.Client.GetTransactionRecordAsync(fx.ContractRecord.Id);
            var createRecord = Assert.IsType<CreateContractRecord>(record);
            Assert.Equal(fx.ContractRecord.Id, createRecord.Id);
            Assert.Equal(fx.ContractRecord.Status, createRecord.Status);
            Assert.Equal(fx.ContractRecord.CurrentExchangeRate, createRecord.CurrentExchangeRate);
            Assert.Equal(fx.ContractRecord.NextExchangeRate, createRecord.NextExchangeRate);
            Assert.Equal(fx.ContractRecord.Hash.ToArray(), createRecord.Hash.ToArray());
            Assert.Equal(fx.ContractRecord.Concensus, createRecord.Concensus);
            Assert.Equal(fx.ContractRecord.Memo, createRecord.Memo);
            Assert.Equal(fx.ContractRecord.Fee, createRecord.Fee);
            Assert.Equal(fx.ContractRecord.Contract, createRecord.Contract);
            Assert.Equal(fx.ContractRecord.CallResult.Error, createRecord.CallResult.Error);
            Assert.Equal(fx.ContractRecord.CallResult.Bloom, createRecord.CallResult.Bloom);
            Assert.Equal(fx.ContractRecord.CallResult.Gas, createRecord.CallResult.Gas);
            Assert.Equal(fx.ContractRecord.CallResult.Events, createRecord.CallResult.Events);
            Assert.Equal(fx.ContractRecord.CallResult.CreatedContracts, createRecord.CallResult.CreatedContracts);
            Assert.Equal(fx.ContractRecord.CallResult.Result.Size, createRecord.CallResult.Result.Size);
        }
        [Fact(DisplayName = "Get Record: Can get Create Account Record")]
        public async Task CanGetCreateAccountRecord()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var record = await fx.Client.GetTransactionRecordAsync(fx.Record.Id);
            var accountRecord = Assert.IsType<CreateAccountRecord>(record);
            Assert.Equal(fx.Record.Id, accountRecord.Id);
            Assert.Equal(fx.Record.Status, accountRecord.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, accountRecord.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, accountRecord.NextExchangeRate);
            Assert.Equal(fx.Record.Hash.ToArray(), accountRecord.Hash.ToArray());
            Assert.Equal(fx.Record.Concensus, accountRecord.Concensus);
            Assert.Equal(fx.Record.Memo, accountRecord.Memo);
            Assert.Equal(fx.Record.Fee, accountRecord.Fee);
            Assert.Equal(fx.Record.Address, accountRecord.Address);
        }
        [Fact(DisplayName = "Get Record: Can get Create File Record")]
        public async Task CanGetCreateFileRecord()
        {
            await using var fx = await TestFile.CreateAsync(_network);
            var record = await fx.Client.GetTransactionRecordAsync(fx.Record.Id);
            var FileRecord = Assert.IsType<FileRecord>(record);
            Assert.Equal(fx.Record.Id, FileRecord.Id);
            Assert.Equal(fx.Record.Status, FileRecord.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, FileRecord.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, FileRecord.NextExchangeRate);
            Assert.Equal(fx.Record.Hash.ToArray(), FileRecord.Hash.ToArray());
            Assert.Equal(fx.Record.Concensus, FileRecord.Concensus);
            Assert.Equal(fx.Record.Memo, FileRecord.Memo);
            Assert.Equal(fx.Record.Fee, FileRecord.Fee);
            Assert.Equal(fx.Record.File, FileRecord.File);
        }
        [Fact(DisplayName = "Get Record: Can get Create Token Record")]
        public async Task CanGetCreateTokenRecord()
        {
            await using var fx = await TestToken.CreateAsync(_network);
            var record = await fx.Client.GetTransactionRecordAsync(fx.Record.Id);
            var createRecord = Assert.IsType<CreateTokenRecord>(record);
            Assert.Equal(fx.Record.Id, createRecord.Id);
            Assert.Equal(fx.Record.Status, createRecord.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, createRecord.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, createRecord.NextExchangeRate);
            Assert.Equal(fx.Record.Hash.ToArray(), createRecord.Hash.ToArray());
            Assert.Equal(fx.Record.Concensus, createRecord.Concensus);
            Assert.Equal(fx.Record.Memo, createRecord.Memo);
            Assert.Equal(fx.Record.Fee, createRecord.Fee);
            Assert.Equal(fx.Record.Token, createRecord.Token);
        }
        [Fact(DisplayName = "Get Record: Can get List of Duplicate Records")]
        public async Task CanGetListOfDuplicateRecords()
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
                var receipts = await client.GetAllTransactionRecordsAsync(txid);
                Assert.Equal(passedPrecheck, receipts.Count);
                Assert.Equal(1, receipts.Count(t => t.Status == ResponseCode.Success));
                Assert.Equal(passedPrecheck - 1, receipts.Count(t => t.Status == ResponseCode.DuplicateTransaction));
                return;
            }
            _network.Output?.WriteLine("TEST INCONCLUSIVE, UNABLE TO CREATE DUPLICATE TRANSACTIONS THIS TIME AROUND.");
        }
        [Fact(DisplayName = "Get Record: Can get List of One Record")]
        public async Task CanGetListOfOneRecord()
        {
            await using var client = _network.NewClient();
            var receipt = await client.TransferAsync(_network.Payer, _network.Gateway, 1);
            var receipts = await client.GetAllTransactionRecordsAsync(receipt.Id);
            Assert.Single(receipts);
            Assert.Equal(ResponseCode.Success, receipts[0].Status);
        }
        [Fact(DisplayName = "Get Record: Can get List of No Records")]
        public async Task CanGetListOfNoRecords()
        {
            await using var client = _network.NewClient();
            var txid = client.CreateNewTxId();
            var receipts = await client.GetAllTransactionRecordsAsync(txid);
            Assert.Empty(receipts);
        }
        [Fact(DisplayName = "Get Record: Can get Create Token Record As List")]
        public async Task CanGetCreateTokenRecordAsList()
        {
            await using var fx = await TestToken.CreateAsync(_network);
            var records = await fx.Client.GetAllTransactionRecordsAsync(fx.Record.Id);
            Assert.Single(records);
            var record = records[0];
            var createRecord = Assert.IsType<CreateTokenRecord>(record);
            Assert.Equal(fx.Record.Id, createRecord.Id);
            Assert.Equal(fx.Record.Status, createRecord.Status);
            Assert.Equal(fx.Record.CurrentExchangeRate, createRecord.CurrentExchangeRate);
            Assert.Equal(fx.Record.NextExchangeRate, createRecord.NextExchangeRate);
            Assert.Equal(fx.Record.Hash.ToArray(), createRecord.Hash.ToArray());
            Assert.Equal(fx.Record.Concensus, createRecord.Concensus);
            Assert.Equal(fx.Record.Memo, createRecord.Memo);
            Assert.Equal(fx.Record.Fee, createRecord.Fee);
            Assert.Equal(fx.Record.Token, createRecord.Token);
        }
        [Fact(DisplayName = "Get Record: Can get TokenRecord for Burn")]
        public async Task CanGetTokenRecordForBurn()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var amountToDestory = fxToken.Params.Circulation / 3;
            var expectedCirculation = fxToken.Params.Circulation - amountToDestory;

            var originalRecord = await fxToken.Client.BurnTokenWithRecordAsync(fxToken, amountToDestory, fxToken.SupplyPrivateKey);
            Assert.Equal(ResponseCode.Success, originalRecord.Status);
            Assert.Equal(expectedCirculation, originalRecord.Circulation);

            var tokenRecord = Assert.IsType<TokenRecord>(originalRecord);
            Assert.Equal(originalRecord.Id, tokenRecord.Id);
            Assert.Equal(originalRecord.Status, tokenRecord.Status);
            Assert.Equal(originalRecord.CurrentExchangeRate, tokenRecord.CurrentExchangeRate);
            Assert.Equal(originalRecord.NextExchangeRate, tokenRecord.NextExchangeRate);
            Assert.Equal(originalRecord.Hash.ToArray(), tokenRecord.Hash.ToArray());
            Assert.Equal(originalRecord.Concensus, tokenRecord.Concensus);
            Assert.Equal(originalRecord.Memo, tokenRecord.Memo);
            Assert.Equal(originalRecord.Fee, tokenRecord.Fee);
            Assert.Equal(originalRecord.Circulation, tokenRecord.Circulation);
        }
        [Fact(DisplayName = "Get Record: Can get TokenRecord for Confiscate")]
        public async Task CanGetTokenRecordForConfiscate()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
            var xferAmount = 2 * fxToken.Params.Circulation / (ulong)Generator.Integer(3, 5);
            var expectedTreasury = fxToken.Params.Circulation - xferAmount;

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            Assert.Equal(xferAmount, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken));
            Assert.Equal(expectedTreasury, await fxAccount.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

            var originalRecord = await fxToken.Client.ConfiscateTokensWithRecordAsync(fxToken, fxAccount, xferAmount, fxToken.ConfiscatePrivateKey);
            Assert.Equal(ResponseCode.Success, originalRecord.Status);
            Assert.Equal(expectedTreasury, originalRecord.Circulation);

            var tokenRecord = Assert.IsType<TokenRecord>(originalRecord);
            Assert.Equal(originalRecord.Id, tokenRecord.Id);
            Assert.Equal(originalRecord.Status, tokenRecord.Status);
            Assert.Equal(originalRecord.CurrentExchangeRate, tokenRecord.CurrentExchangeRate);
            Assert.Equal(originalRecord.NextExchangeRate, tokenRecord.NextExchangeRate);
            Assert.Equal(originalRecord.Hash.ToArray(), tokenRecord.Hash.ToArray());
            Assert.Equal(originalRecord.Concensus, tokenRecord.Concensus);
            Assert.Equal(originalRecord.Memo, tokenRecord.Memo);
            Assert.Equal(originalRecord.Fee, tokenRecord.Fee);
            Assert.Equal(originalRecord.Circulation, tokenRecord.Circulation);
        }
        [Fact(DisplayName = "Get Record: Can get TokenRecord for Mint")]
        public async Task CanGetTokenRecordForMint()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var originalRecord = await fxToken.Client.MintTokenWithRecordAsync(fxToken.Record.Token, fxToken.Params.Circulation, fxToken.SupplyPrivateKey);
            Assert.Equal(ResponseCode.Success, originalRecord.Status);
            Assert.Equal(fxToken.Params.Circulation * 2, originalRecord.Circulation);

            var tokenRecord = Assert.IsType<TokenRecord>(originalRecord);
            Assert.Equal(originalRecord.Id, tokenRecord.Id);
            Assert.Equal(originalRecord.Status, tokenRecord.Status);
            Assert.Equal(originalRecord.CurrentExchangeRate, tokenRecord.CurrentExchangeRate);
            Assert.Equal(originalRecord.NextExchangeRate, tokenRecord.NextExchangeRate);
            Assert.Equal(originalRecord.Hash.ToArray(), tokenRecord.Hash.ToArray());
            Assert.Equal(originalRecord.Concensus, tokenRecord.Concensus);
            Assert.Equal(originalRecord.Memo, tokenRecord.Memo);
            Assert.Equal(originalRecord.Fee, tokenRecord.Fee);
            Assert.Equal(originalRecord.Circulation, tokenRecord.Circulation);
        }
    }
}
