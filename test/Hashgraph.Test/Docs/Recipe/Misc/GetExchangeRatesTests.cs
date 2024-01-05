#pragma warning disable CS8892 //Main will not be used as an entry point
namespace Hashgraph.Tests.Docs.Recipe;

[Collection(nameof(NetworkCredentials))]
public class GetExchangeRatesTests
{
    // Code Example:  Docs / Recipe / Misc / Get Exchange Rates
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = new Uri(args[0]);            //   http://2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
        var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (Ed25519 private in hex)
        try
        {
            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                ctx.Payer = new Address(0, 0, payerAccountNo);
                ctx.Signatory = new Signatory(payerPrivateKey);
            });

            var rate = await client.GetExchangeRatesAsync();

            Console.Write($"Current: cent/hBar = {rate.Current.USDCentEquivalent}");
            Console.Write($"/{rate.Current.HBarEquivalent}");
            Console.Write($"  Expires {rate.Current.Expiration}");
            Console.WriteLine();
            Console.Write($"Next: cent/hBar = {rate.Next.USDCentEquivalent}");
            Console.Write($"/{rate.Next.HBarEquivalent}");
            Console.Write($"  Expires {rate.Next.Expiration}");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }

    private readonly NetworkCredentials _network;
    public GetExchangeRatesTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }

    [Fact(DisplayName = "Docs Recipe Example: Get Exchange Rates")]
    public async Task RunTest()
    {
        using (new ConsoleRedirector(_network.Output))
        {
            var arg0 = _network.Gateway.Uri;
            var arg1 = _network.Gateway.AccountNum.ToString();
            var arg2 = _network.Payer.AccountNum.ToString();
            var arg3 = Hex.FromBytes(_network.PrivateKey);
            await Main(new string[] { arg0.ToString(), arg1, arg2, arg3 });
        }
    }
}