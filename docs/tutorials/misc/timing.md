---
title: Get Transaction Execution Time
layout: default
---

# Get Transaction Execution Time

Queries the network for the amount of time it took to process one or more transactions.

```csharp
class Program
{
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
        var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (Ed25519 private in hex)
        var txAccountNum = long.Parse(args[4]);       //   Transaction Account Payer Num
        var txStartingSeconds = long.Parse(args[5]);  //   Transaction Starting Seconds (Epoch)
        var txStartingNanos = int.Parse(args[6]);      //   Transaction Starting Nanoseconds
        try
        {
            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                ctx.Payer = new Address(0, 0, payerAccountNo);
                ctx.Signatory = new Signatory(payerPrivateKey);
            });

            var txAddress = new Address(0, 0, txAccountNum);
            var txId = new TxId(txAddress, txStartingSeconds, txStartingNanos);
            var txIds = new[] { txId };

            var timings = await client.GetExecutionTimes(txIds);
            var executionTime = timings.First();

            Console.WriteLine($"Transaction Execution Time in Nanoseconds: {executionTime}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }
}
```
