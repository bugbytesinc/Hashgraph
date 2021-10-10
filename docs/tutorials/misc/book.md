---
title: Get Network Address Book
layout: default
---

# Get Network Address Book

Queries the network for the book of network node addresses and information.

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

            var nodes = await client.GetAddressBookAsync();
            foreach (var node in nodes)
            {
                Console.Write($"Node {node.Id}: ");
                Console.Write($"{node.Address.ShardNum}.");
                Console.Write($"{node.Address.RealmNum}.");
                Console.WriteLine($"{node.Address.AccountNum}:");
                foreach (var endpoint in node.Endpoints)
                {
                    var address = new IPAddress(endpoint.IpAddress.ToArray());
                    var port = endpoint.Port;
                    Console.WriteLine($"     {address}:{port}");
                }
            }
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
Node 0: 0.0.3:
     35.231.208.148:50211
     35.231.208.148:50212
     3.211.248.172:50211
     3.211.248.172:50212
     40.121.64.48:50211
     40.121.64.48:50212
Node 1: 0.0.4:
     35.199.15.177:50211
     35.199.15.177:50212
     3.133.213.146:50211
     3.133.213.146:50212
     40.70.11.202:50211
     40.70.11.202:50212
Node 2: 0.0.5:
     35.225.201.195:50211
     35.225.201.195:50212
     52.15.105.130:50211
     52.15.105.130:50212
     104.43.248.63:50211
     104.43.248.63:50212
Node 3: 0.0.6:
     35.247.109.135:50211
     35.247.109.135:50212
     54.241.38.1:50211
     54.241.38.1:50212
     13.88.22.47:50211
     13.88.22.47:50212
Node 4: 0.0.7:
     35.235.65.51:50211
     35.235.65.51:50212
     54.177.51.127:50211
     54.177.51.127:50212
     13.64.170.40:50211
     13.64.170.40:50212
Node 5: 0.0.8:
     34.106.247.65:50211
     34.106.247.65:50212
     35.83.89.171:50211
     35.83.89.171:50212
     13.78.232.192:50211
     13.78.232.192:50212
Node 6: 0.0.9:
     34.125.23.49:50211
     34.125.23.49:50212
     50.18.17.93:50211
     50.18.17.93:50212
     20.150.136.89:50211
     20.150.136.89:50212
```
