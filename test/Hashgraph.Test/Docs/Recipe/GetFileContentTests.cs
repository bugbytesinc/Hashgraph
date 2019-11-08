using Hashgraph.Test.Fixtures;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Tests.Docs.Recipe
{
    [Collection(nameof(NetworkCredentials))]
    public class GetFileContentTests
    {
        // Code Example:  Docs / Recipe / Get File Contents
        static async Task Main(string[] args)
        {                                                 // For Example:
            var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
            var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
            var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
            var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (48 byte Ed25519 private in hex)
            var fileNo = long.Parse(args[4]);             //   1234 (account 0.0.1234)
            try
            {
                await using var client = new Client(ctx =>
                {
                    ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                    ctx.Payer = new Address(0, 0, payerAccountNo);
                    ctx.Signatory = new Signatory(payerPrivateKey);
                });
                var file = new Address(0, 0, fileNo);
                var bytes = await client.GetFileContentAsync(file);
                Console.Write(Encoding.UTF8.GetString(bytes.ToArray()));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        private readonly NetworkCredentials _network;
        public GetFileContentTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }

        [Fact(DisplayName = "Docs Recipe Example: Get File Contents")]
        public async Task RunTest()
        {
            await using var client = _network.NewClient();
            var file = await client.CreateFileAsync(new CreateFileParams
            {
                Expiration = DateTime.UtcNow.AddSeconds(7890000),
                Contents = Encoding.UTF8.GetBytes("Hello Hedera"),
                Endorsements = new Endorsement[] { _network.PublicKey }
            });

            using (new ConsoleRedirector(_network.Output))
            {
                var arg0 = _network.Gateway.Url;
                var arg1 = _network.Gateway.AccountNum.ToString();
                var arg2 = _network.Payer.AccountNum.ToString();
                var arg3 = Hex.FromBytes(_network.PrivateKey);
                var arg4 = file.File.AccountNum.ToString();
                await Main(new string[] { arg0, arg1, arg2, arg3, arg4 });
            }
        }
    }
}
