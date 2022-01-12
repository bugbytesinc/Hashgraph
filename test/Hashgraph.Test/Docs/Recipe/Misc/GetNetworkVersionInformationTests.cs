#pragma warning disable CS8892 //Main will not be used as an entry point
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Tests.Docs.Recipe;

[Collection(nameof(NetworkCredentials))]
public class GetNetworkVersionInformationTests
{
    // Code Example:  Docs / Recipe / Misc / Get Network Version Information
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
        var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (48 byte Ed25519 private in hex)
        try
        {
            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                ctx.Payer = new Address(0, 0, payerAccountNo);
                ctx.Signatory = new Signatory(payerPrivateKey);
            });

            var info = await client.GetVersionInfoAsync();

            Console.WriteLine($"HAPI Version:     " +
                info.ApiProtobufVersion.Major + "." +
                info.ApiProtobufVersion.Minor + "." +
                info.ApiProtobufVersion.Patch);
            Console.WriteLine("Services Version: " +
                info.HederaServicesVersion.Major + "." +
                info.HederaServicesVersion.Minor + "." +
                info.HederaServicesVersion.Patch);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }

    private readonly NetworkCredentials _network;
    public GetNetworkVersionInformationTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }

    [Fact(DisplayName = "Docs Recipe Example: Get Account Info")]
    public async Task RunTest()
    {
        using (new ConsoleRedirector(_network.Output))
        {
            var arg0 = _network.Gateway.Url;
            var arg1 = _network.Gateway.AccountNum.ToString();
            var arg2 = _network.Payer.AccountNum.ToString();
            var arg3 = Hex.FromBytes(_network.PrivateKey);
            await Main(new string[] { arg0, arg1, arg2, arg3 });
        }
    }
}