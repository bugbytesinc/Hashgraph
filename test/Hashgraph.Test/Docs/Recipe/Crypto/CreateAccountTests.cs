#pragma warning disable CS8892 //Main will not be used as an entry point
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Tests.Docs.Recipe;

[Collection(nameof(NetworkCredentials))]
public class CreateAccountTests
{
    // Code Example:  Docs / Recipe / Crypto / Creeate Account
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = new Uri(args[0]);            //   http://2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
        var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (48 byte Ed25519 private in hex)
        var newPublicKey = Hex.ToBytes(args[4]);      //   302a3005... (44 byte Ed25519 public in hex)
        var initialBalance = ulong.Parse(args[5]);    //   100_000_000 (1ℏ initial balance)
        try
        {
            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                ctx.Payer = new Address(0, 0, payerAccountNo);
                ctx.Signatory = new Signatory(payerPrivateKey);
            });
            var createParams = new CreateAccountParams
            {
                Endorsement = new Endorsement(newPublicKey),
                InitialBalance = initialBalance
            };
            var account = await client.CreateAccountAsync(createParams);
            var address = account.Address;
            Console.WriteLine($"New Account ID: {address.ShardNum}.{address.RealmNum}.{address.AccountNum}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }

    private readonly NetworkCredentials _network;
    public CreateAccountTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }

    [Fact(DisplayName = "Docs Recipe Example: Create Account")]
    public async Task RunTest()
    {
        var (publicKey, _) = Generator.KeyPair();
        using (new ConsoleRedirector(_network.Output))
        {
            var arg0 = _network.Gateway.Uri;
            var arg1 = _network.Gateway.AccountNum.ToString();
            var arg2 = _network.Payer.AccountNum.ToString();
            var arg3 = Hex.FromBytes(_network.PrivateKey);
            var arg4 = Hex.FromBytes(publicKey);
            var arg5 = "1";
            await Main(new string[] { arg0.ToString(), arg1, arg2, arg3, arg4, arg5 });
        }
    }
}