#pragma warning disable CS8892 //Main will not be used as an entry point
using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Tests.Docs.Recipe
{
    [Collection(nameof(NetworkCredentials))]
    public class GetTransactionExecutionTimeTests
    {
        // Code Example:  Docs / Recipe / Misc / Get Transaction Execution Time
        static async Task Main(string[] args)
        {                                                 // For Example:
            var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
            var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
            var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
            var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (Ed25519 private in hex)
            var txAccountNum = long.Parse(args[4]);       //   Transaction Account Payer Num
            var txStartingSeconds = long.Parse(args[5]);  //   Transaction Starting Seconds (Epoch)
            var txStartingNanos = int.Parse(args[6]);      //   Transaction Starting Nanoseconds
            try
            {
                await using var client = new Client(ctx =>
                {
                    ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                    ctx.Payer = new Address(0, 0, payerAccountNo);
                    ctx.Signatory = new Signatory(payerPrivateKey);
                });

                var txAddress = new Address(0, 0, txAccountNum);
                var txId = new TxId(txAddress, txStartingSeconds, txStartingNanos);
                var txIds = new[] { txId };

                var timings = await client.GetExecutionTimes(txIds);
                var executionTime = timings.First();

                Console.WriteLine($"Transaction Execution Time in Nanoseconds: {executionTime}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        private readonly NetworkCredentials _network;
        public GetTransactionExecutionTimeTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }

        [Fact(DisplayName = "Docs Recipe Example: Get Transaction Execution Time Tests")]
        public async Task RunTest()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            using (new ConsoleRedirector(_network.Output))
            {
                var arg0 = _network.Gateway.Url;
                var arg1 = _network.Gateway.AccountNum.ToString();
                var arg2 = _network.Payer.AccountNum.ToString();
                var arg3 = Hex.FromBytes(_network.PrivateKey);
                var arg4 = fxAccount.Record.Id.Address.AccountNum.ToString();
                var arg5 = fxAccount.Record.Id.ValidStartSeconds.ToString();
                var arg6 = fxAccount.Record.Id.ValidStartNanos.ToString();
                await Main(new string[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6 });
            }
        }
    }
}
