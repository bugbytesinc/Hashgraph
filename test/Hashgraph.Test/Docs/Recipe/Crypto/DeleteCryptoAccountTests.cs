#pragma warning disable CS8892 //Main will not be used as an entry point
namespace Hashgraph.Tests.Docs.Recipe;

[Collection(nameof(NetworkCredentials))]
public class DeleteCryptoAccountTests
{
    // Code Example:  Docs / Recipe / Delete Crypto Account
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = new Uri(args[0]);            //   http://k2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
        var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (Ed25519 private in hex)
        var deleteAccountNo = long.Parse(args[4]);    //   2300 (account 0.0.2300)
        var deleteAccountKey = Hex.ToBytes(args[5]);  //   302e0201... (Ed25519 private in hex)
        try
        {
            var payerAccount = new Address(0, 0, payerAccountNo);
            var payerSignatory = new Signatory(payerPrivateKey);
            var accountToDelete = new Address(0, 0, deleteAccountNo);
            var deleteAccountSignatory = new Signatory(deleteAccountKey);

            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                ctx.Payer = payerAccount;
                ctx.Signatory = payerSignatory;
            });

            var receipt = await client.DeleteAccountAsync(
                accountToDelete,
                payerAccount,
                deleteAccountSignatory);
            Console.WriteLine($"Status: {receipt.Status}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }

    private readonly NetworkCredentials _network;
    public DeleteCryptoAccountTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }

    [Fact(DisplayName = "Docs Recipe Example: Delete Crypto Account")]
    public async Task RunTest()
    {
        await using var fxFrom = await TestAccount.CreateAsync(_network);
        await using var fxTo = await TestAccount.CreateAsync(_network);
        using (new ConsoleRedirector(_network.Output))
        {
            var arg0 = _network.Gateway.Uri;
            var arg1 = _network.Gateway.AccountNum.ToString();
            var arg2 = _network.Payer.AccountNum.ToString();
            var arg3 = Hex.FromBytes(_network.PrivateKey);
            var arg4 = fxFrom.Record.Address.AccountNum.ToString();
            var arg5 = Hex.FromBytes(fxFrom.PrivateKey);
            var arg6 = fxTo.Record.Address.AccountNum.ToString();
            var arg7 = (fxFrom.CreateParams.InitialBalance / 2).ToString();
            await Main(new string[] { arg0.ToString(), arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
        }
    }
}