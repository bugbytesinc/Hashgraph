---
title: Delete Crypto Account
---

# Delete Crypto Account

Delete a crypto account, sending the remaining balance of hBars and tokens to the specified crypto account.

Example:

```csharp
class Program
{
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
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
}
```
