---
title: Transfer Crypto
---

# Transfer Crypto

In preparation for transferring crypto from one account to another, the first step is to create a Hashgraph [`Client`](xref:Hashgraph.Client) object.  The [`Client`](xref:Hashgraph.Client) object orchestrates the transfer request construction and communication with the hedera network. It requires a small amount of configuration when created. At a minimum to transfer crypto, the client must be configured with a [`Gateway`](xref:Hashgraph.Gateway) and [`Payer`](xref:Hashgraph.IContext.Payer). The [`Gateway`](xref:Hashgraph.Gateway) object represents the internet network address and account for the node processing the request and the [`Payer`](xref:Hashgraph.IContext.Payer) Account represents the account that will sign and pay the crypto transfer processing fees.  The [`Payer`](xref:Hashgraph.IContext.Payer) consists of two things: an [`Address`](xref:Hashgraph.Address) identifying the account paying transaction fees; and a [`Signatory`](xref:Hashgraph.Signatory) holding the signing key associated with the [`Payer`](xref:Hashgraph.IContext.Payer) account.  

The next step is to identify the account to debit (send funds from) and the account to credit (send funds to).  The debit account must also sign the crypto transfer transaction (which may or may not be the same account as the Payer) and provide a [`Signatory`](xref:Hashgraph.Signatory) holding the signing key associated with the debit account.  In some cases, the credit account may also require a signature to accept funds, but for this example we will assume this is not the case.  The amount transfer is denoted in [_tinybars_](https://help.hedera.com/hc/en-us/articles/360000674317-What-are-the-official-HBAR-cryptocurrency-denominations-).  After creating and configuring the client object, the [`TransferAsync`](xref:Hashgraph.Client.TransferAsync(Hashgraph.Address,Hashgraph.Address,System.Int64,System.Action{Hashgraph.IContext})) method submits the request to the network and returns a [`Receipt`](xref:Hashgraph.TransactionReceipt) indicating success or failure of the request.  The following code example illustrates a small program performing these actions:


```csharp
class Program
{
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
        var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (48 byte Ed25519 private in hex)
        var fromAccountNo = long.Parse(args[4]);      //   2300 (account 0.0.2300)
        var fromPrivateKey = Hex.ToBytes(args[5]);    //   302e0201... (48 byte Ed25519 private in hex)
        var toAccountNo = long.Parse(args[6]);        //   4500 (account 0.0.4500)
        var amount = long.Parse(args[7]);             //   100000000 (1 hBar)
        try
        {
            var fromAccount = new Address(0, 0, fromAccountNo);
            var fromSignatory = new Signatory(fromPrivateKey);
            var toAccount = new Address(0, 0, toAccountNo);

            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                ctx.Payer = new Address(0, 0, payerAccountNo);
                ctx.Signatory = new Signatory(payerPrivateKey);
            });

            var receipt = await client.TransferAsync(fromAccount, toAccount, amount, fromSignatory);
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


While outside the scope of this example, it is possible to create a signatory that invokes an external method to sign the crypto transfer transaction instead; this is useful for scenarios where the private key is held outside of the system using this library. Thru this mechanism it is possible for the library to never see a private signing key.

