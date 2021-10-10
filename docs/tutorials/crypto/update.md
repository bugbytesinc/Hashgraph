---
title: Update Crypto Account
---

# Update Crypto Account

Update the properties of a crypto account.

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
}
```
