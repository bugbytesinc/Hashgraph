---
title: Get File Content
layout: default
---

# Get File Content

In preparation for retrieving the contents of a file, the first step is to create a Hashgraph [`Client`](xref:Hashgraph.Client) object.  The [`Client`](xref:Hashgraph.Client) object orchestrates the request construction and communication with the hedera network. It requires a small amount of configuration when created. At a minimum to retrieve the information, the client must be configured a [`Gateway`](xref:Hashgraph.Gateway) and [`Payer`](xref:Hashgraph.IContext.Payer). The [`Gateway`](xref:Hashgraph.Gateway) object represents the internet network address and account for the node processing requests and the [`Payer`](xref:Hashgraph.IContext.Payer) Account represents the account that will sign and pay for the query.  The [`Payer`](xref:Hashgraph.IContext.Payer) consists of two things: and [`Address`](xref:Hashgraph.Address) identifying the account paying transaction fees for the request; and a [`Signatory`](xref:Hashgraph.Signatory) holding the signing key associated with the [`Payer`](xref:Hashgraph.IContext.Payer) account.  Retrieving the contents of a file is not free, the network requires a nominal payment of a few [_tinybars_](https://help.hedera.com/hc/en-us/articles/360000674317-What-are-the-official-HBAR-cryptocurrency-denominations-) from the paying account to process the request.  After creating and configuring the client object, the [`GetFileContentAsync`](xref:Hashgraph.Client.GetFileContentAsync(Hashgraph.Address,System.Action{Hashgraph.IContext})) method submits the request to the network and returns an [`ReadOnlyMemory<byte>`](xref:System.ReadOnlyMemory`1) object that wraps the byte contents representing the file.  The contents may be further encoded in some manner, such as ASCII or as a serialized protobuf message.  The encoding is specific to the application using the file.  The following code example illustrates a small program that retrieves a file that happens to be UTF-8 encoded:

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
                ctx.Payer = new Address(0, 0, payerAccountNo);
                ctx.Signatory = new Signatory(payerPrivateKey);
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

One should note that to create a [`Signatory`](xref:Hashgraph.Signatory) associated with the payer account, one does need to have access to the account’s private key(s) to sign the transaction authorizing payment to the network query request.  For the file being queried, only the address need be known.  

While outside the scope of this example, it is possible to create a signatory that invokes an external method to sign the transaction instead; this is useful for scenarios where the private key is held outside of the system using this library.  Thru this mechanism it is possible for the library to _never_ see a private signing key.
