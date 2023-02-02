#pragma warning disable CS8892 //Main will not be used as an entry point
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Tests.Docs.Recipe;

[Collection(nameof(NetworkCredentials))]
public class UpdateAccountTests
{
    // Code Example:  Docs / Recipe / Crypto / Update Account
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = new Uri(args[0]);            //   http://2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
        var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (Ed25519 private in hex)
        var targetAccountNo = long.Parse(args[4]);    //   2023 (account 0.0.2023)
        var targetPrivateKey = Hex.ToBytes(args[5]);  //   302e0201... (Ed25519 private in hex)
        var targetAccountNewMemo = args[6];           //   New Memo to Associate with Target
        try
        {
            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                ctx.Payer = new Address(0, 0, payerAccountNo);
                ctx.Signatory = new Signatory(payerPrivateKey);
            });

            var updateParams = new UpdateAccountParams
            {
                Address = new Address(0, 0, targetAccountNo),
                Signatory = new Signatory(targetPrivateKey),
                Memo = targetAccountNewMemo
            };

            var receipt = await client.UpdateAccountAsync(updateParams);
            Console.WriteLine($"Status: {receipt.Status}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }

    private readonly NetworkCredentials _network;
    public UpdateAccountTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }

    [Fact(DisplayName = "Docs Recipe Example: Update Account")]
    public async Task RunTest()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        using (new ConsoleRedirector(_network.Output))
        {
            var arg0 = _network.Gateway.Uri;
            var arg1 = _network.Gateway.AccountNum.ToString();
            var arg2 = _network.Payer.AccountNum.ToString();
            var arg3 = Hex.FromBytes(_network.PrivateKey);
            var arg4 = fxAccount.Record.Address.AccountNum.ToString();
            var arg5 = Hex.FromBytes(fxAccount.PrivateKey);
            var arg6 = Generator.String(10, 20);
            await Main(new string[] { arg0.ToString(), arg1, arg2, arg3, arg4, arg5, arg6 });
        }
    }
}