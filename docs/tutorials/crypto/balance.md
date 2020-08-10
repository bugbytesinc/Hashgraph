---
title: Get Account Balance
---

# Get Account Balance

In preparation for querying an account for its crypto balance, the first step is to create a Hashgraph [`Client`](xref:Hashgraph.Client) object.  The [`Client`](xref:Hashgraph.Client) object orchestrates the request construction and communication with the hedera network. It requires a small amount of configuration when created. At a minimum to retrieve an account balance, the client must be configured with a [`Gateway`](xref:Hashgraph.Gateway). The [`Gateway`](xref:Hashgraph.Gateway) object represents the internet network address and account for the node processing requests. Querying the balance of an account is is free. After creating and configuring the client object, the [`GetAccountBalanceAsync`](xref:Hashgraph.Client.GetAccountBalanceAsync(Hashgraph.Address,System.Action{Hashgraph.IContext})) method submits the request to the network and returns the balance of the account in [_tinybars_](https://help.hedera.com/hc/en-us/articles/360000674317-What-are-the-official-HBAR-cryptocurrency-denominations-).  The following code example illustrates a small program performing these actions:

```csharp
class Program
{
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var queryAccountNo = long.Parse(args[2]);     //   2300 (account 0.0.2300)
        try
        {
            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
            });
            var account = new Address(0, 0, queryAccountNo);
            var balance = await client.GetAccountBalanceAsync(account);
            Console.WriteLine($"Account Balance for {account.AccountNum} is {balance:#,#} tinybars.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }
}
```
