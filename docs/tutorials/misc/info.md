---
title: Get Network Version Information
layout: default
---

# Get Network Version Information

Queries the network for version information identifying the Hedera Services version API Protobuf version implemented by the node being queried.

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
}
```
Example Output:
```
    HAPI Version:     0.18.1
    Services Version: 0.18.1
```