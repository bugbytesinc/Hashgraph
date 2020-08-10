---
title: Create New Account
---

# Create New Account

All accounts within the Hedera network can create new accounts.  To create a new account, the existing account holder must provide to the hedera network a public key matching a private key that will secure the funds held by the new account.  The holder of the new account must protect this private key, losing control of this private key will result in a loss of control of the created account.  The creating account holder must also decide on how much crypto to provide to the account for its initial balance.  The network will generate the “account id” in the form of 0.0.x (shard.realm.number).  This is the identifier for the newly created account (unlike other systems where the public key is the identifier).

## Example

The first step in this process is to create a Hashgraph [`Client`](xref:Hashgraph.Client) object.  The [`Client`](xref:Hashgraph.Client) object orchestrates the request construction and communication with the hedera network. It requires a small amount of configuration when created.  At a minimum to create the new account, the client must be configured with a [`Gateway`](xref:Hashgraph.Gateway) and [`Payer`](xref:Hashgraph.IContext.Payer). The [`Gateway`](xref:Hashgraph.Gateway) object represents the internet network address and account for the node processing the transaction request, and the [`Payer`](xref:Hashgraph.IContext.Payer) Account identifies the account that will sign and pay for the transaction.  The [`Payer`](xref:Hashgraph.IContext.Payer) consists of two things: an [`Address`](xref:Hashgraph.Address) identifying the account paying transaction fees (which includes the value of the account’s initial balance); and a [`Signatory`](xref:Hashgraph.Signatory) holding the signing key associated with the Payer account.  

The next step is to create a [`CreateAccountParams`](xref:Hashgraph.CreateContractParams) object; it holds the details of the create request.  The two most important properties to set on this object are the [`Endorsement`](xref:Hashgraph.CreateAccountParams.Endorsement) and [`InitialBalance`](xref:Hashgraph.CreateAccountParams.InitialBalance) properties.  In the simplest case, the endorsement is a single Ed25519 public key (discussed above).  The value of the initial balance will be drawn from the payer account and placed into the newly created account.  The default values for the remaining properties need not be altered.

Finally, to create the Hedera Account, invoke the client’s [`CreateAccountAsync`](xref:Hashgraph.Client.CreateAccountAsync(Hashgraph.CreateAccountParams,System.Action{Hashgraph.IContext})) method to submit the request to the network.  The method returns a [`CreateAccountReceipt`](xref:Hashgraph.CreateAccountReceipt) containing a property, [`Address`](xref:Hashgraph.Address), identifying the newly created account.   The following code example illustrates a small program performing these actions:

```csharp
class Program
{
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
        var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (48 byte Ed25519 private in hex)
        var newPublicKey = Hex.ToBytes(args[4]);      //   302a3005... (44 byte Ed25519 public in hex)
        var initialBalance = ulong.Parse(args[5]);    //   100_000_000 (1ℏ initial balance)
        try
        {
            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                ctx.Payer = new Address(0, 0, payerAccountNo);
                ctx.Signatory = new Signatory(payerPrivateKey);
            });
            var createParams = new CreateAccountParams
            {
                Endorsement = new Endorsement(newPublicKey),
                InitialBalance = initialBalance
            };
            var account = await client.CreateAccountAsync(createParams);
            var address = account.Address;
            Console.WriteLine(
                $"New Account ID: {address.ShardNum}.{address.RealmNum}.{address.AccountNum}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }
}
```
