---
title: Get Exchange Rates
layout: default
---

# Get Exchange Rates

Queries the network for current and 'next' USD/hBar echange rate (for fee payment calculations).

Note: this information can be retrieved from any recently executed receipt as well.

```csharp
class Program
{
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
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
}
```
Example output:
```txt
    Current: cent/hBar = 12/1  Expires 1/1/2100 12:00:00 AM
    Next: cent/hBar = 15/1  Expires 1/1/2100 12:00:00 AM
```
