#pragma warning disable CS8892 //Main will not be used as an entry point
using Google.Protobuf;
using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Tests.Docs.Recipe
{
    [Collection(nameof(NetworkCredentials))]
    public class SubmitUnsafeTransactionTests
    {
        // Code Example:  Docs / Recipe / Misc / Submit Unsafe Tx
        static async Task Main(string[] args)
        {                                                 // For Example:
            var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
            var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
            var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
            var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (Ed25519 private in hex)
            var transactionBytes = Hex.ToBytes(args[4]);  //   Hex Encoded Signed Transaction
            try
            {
                await using var client = new Client(ctx =>
                {
                    ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                    ctx.Payer = new Address(0, 0, payerAccountNo);
                    ctx.Signatory = new Signatory(payerPrivateKey);
                });

                var receipt = await client.SubmitUnsafeTransactionAsync(transactionBytes);
                Console.WriteLine($"Status: {receipt.Status}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        private readonly NetworkCredentials _network;
        public SubmitUnsafeTransactionTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }

        [Fact(DisplayName = "Docs Recipe Example: Submit Unsafe Transaction")]
        public async Task RunTest()
        {
            await using var client = _network.NewClient();

            // Build a TX from Scratch, Including a Signature
            var txid = client.CreateNewTxId();
            var transfers = new Proto.TransferList();
            transfers.AccountAmounts.Add(new Proto.AccountAmount
            {
                AccountID = new Proto.AccountID(_network.Payer),
                Amount = -1
            });
            transfers.AccountAmounts.Add(new Proto.AccountAmount
            {
                AccountID = new Proto.AccountID(_network.Gateway),
                Amount = 1
            });
            var body = new Proto.TransactionBody
            {
                TransactionID = new Proto.TransactionID(txid),
                NodeAccountID = new Proto.AccountID(_network.Gateway),
                TransactionFee = 30_00_000_000,
                TransactionValidDuration = new Proto.Duration { Seconds = 180 },
                Memo = "Unsafe Test",
                CryptoTransfer = new Proto.CryptoTransferTransactionBody { Transfers = transfers }
            };
            var invoice = new Invoice(body, 6);
            await (_network.Signatory as ISignatory).SignAsync(invoice);
            var transaction = new Proto.Transaction
            {
                SignedTransactionBytes = invoice.GenerateSignedTransactionFromSignatures().ToByteString()
            };

            using (new ConsoleRedirector(_network.Output))
            {
                var arg0 = _network.Gateway.Url;
                var arg1 = _network.Gateway.AccountNum.ToString();
                var arg2 = _network.Payer.AccountNum.ToString();
                var arg3 = Hex.FromBytes(_network.PrivateKey);
                var arg4 = Hex.FromBytes(transaction.ToByteArray());
                await Main(new string[] { arg0, arg1, arg2, arg3, arg4 });
            }
        }
    }
}
