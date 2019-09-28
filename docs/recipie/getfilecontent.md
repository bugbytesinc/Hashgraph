---
title: Get File Content
layout: default
---

# Get File Content

In preparation for retrieving the contents of a file, the first step is to create a Hashgraph `Client` object.  The Client object orchestrates the request construction and communication with the hedera network. It requires a small amount of configuration when created. At a minimum to retrieve the information, the client must be configured with a `Gateway` and Payer `Account`. The Gateway object represents the internet network address and account for the node processing requests and the Payer Account represents the account that will sign and pay for the query. Note, retreiving the contents of a file is not free, the network requires a nominal payment of a few _tinybars_ from the paying account to process the request.  After creating and configuring the client object, the `GetFileContentAsync` method submits the request to the network and returns an `ReadOnlyMemory<byte>` object that wraps the byte contents representing the file.  The contents may be further encoded in some manner, such as ASCII or as a serialized protobuf message.  The encoding is specific to the application using the file.  The following code example illustrates a small program that retrieves a file that happens to be UTF-8 encoded:

```csharp
class Program
{
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
        var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (48 byte Ed25519 private in hex)
        var fileNo = long.Parse(args[4]);             //   1234 (account 0.0.1234)
        try
        {
            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                ctx.Payer = new Account(0, 0, payerAccountNo, payerPrivateKey);
            });
            var file = new Address(0, 0, fileNo);
            var bytes = await client.GetFileContentAsync(file);
            Console.Write(Encoding.UTF8.GetString(bytes.ToArray()));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }
}
```

One should note that to construct a payer account, one does need to have access to the necessary private keys to sign the transaction to pay for the network query request (as well as the paying account’s address number).
