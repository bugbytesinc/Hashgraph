#pragma warning disable CS8892 //Main will not be used as an entry point
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Tests.Docs.Recipe
{
    [Collection(nameof(NetworkCredentials))]
    public class TransferCryptoTests
    {
        // Code Example:  Docs / Recipe / Transfer Crypto
        static async Task Main(string[] args)
        {                                                 // For Example:
            var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
            var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
            var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
            var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (48 byte Ed25519 private in hex)
            var fromAccountNo = long.Parse(args[4]);      //   2300 (account 0.0.2300)
            var fromPrivateKey = Hex.ToBytes(args[5]);    //   302e0201... (48 byte Ed25519 private in hex)
            var toAccountNo = long.Parse(args[6]);        //   4500 (account 0.0.4500)
            var amount = long.Parse(args[7]);             //   100000000 (1 hBar)
            try
            {
                var fromAccount = new Address(0, 0, fromAccountNo);
                var fromSignatory = new Signatory(fromPrivateKey);
                var toAccount = new Address(0, 0, toAccountNo);

                await using var client = new Client(ctx =>
                {
                    ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                    ctx.Payer = new Address(0, 0, payerAccountNo);
                    ctx.Signatory = new Signatory(payerPrivateKey);
                });

                var receipt = await client.TransferAsync(fromAccount, toAccount, amount, fromSignatory);
                Console.WriteLine($"Status: {receipt.Status}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        private readonly NetworkCredentials _network;
        public TransferCryptoTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }

        [Fact(DisplayName = "Docs Recipe Example: Transfer Crypto")]
        public async Task RunTest()
        {
            await using var fxFrom = await TestAccount.CreateAsync(_network);
            await using var fxTo = await TestAccount.CreateAsync(_network);
            using (new ConsoleRedirector(_network.Output))
            {
                var arg0 = _network.Gateways[0].Url;
                var arg1 = _network.Gateways[0].AccountNum.ToString();
                var arg2 = _network.Payer.AccountNum.ToString();
                var arg3 = Hex.FromBytes(_network.PrivateKey);
                var arg4 = fxFrom.Record.Address.AccountNum.ToString();
                var arg5 = Hex.FromBytes(fxFrom.PrivateKey);
                var arg6 = fxTo.Record.Address.AccountNum.ToString();
                var arg7 = (fxFrom.CreateParams.InitialBalance / 2).ToString();
                await Main(new string[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
            }
        }
    }
}
