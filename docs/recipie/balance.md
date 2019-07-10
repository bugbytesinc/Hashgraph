---
title: Get Account Balance
layout: default
---

# Get Account Balance

In preparation for querying an account for its crypto balance, the first step is to create a Hashgraph `Client` object.  The Client object orchestrates the request construction and communication with the hedera network. It requires a small amount of configuration when created. At a minimum to retrieve an account balance, the client must be configured with a `Gateway` and Payer `Account`. The Gateway object represents the internet network address and account for the node processing requests and the Payer Account represents the account that will sign and pay for the query. Note, querying the balance of an account is not free, the network requires a payment of 100,000 _tinybars_ from the paying account to process the request.  After creating and configuring the client object, the `GetAccountBalanceAsync` method submits the request to the network and returns the balance of the account in _tinybars_.  The following code example illustrates a small program performing these actions:

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var client = new Client(ctx => {
                ctx.Gateway = new Gateway("0.testnet.hedera.com:50211", 0, 0, 3);
                ctx.Payer = new Account(0, 0, <PAYER ACCOUNT NUMBER>, <PRIVATE KEY IN HEX>);
            });
            var account = new Address(0, 0, 1020);
            var balance = await client.GetAccountBalanceAsync(account);
            Console.WriteLine($"Account Balance for {account.AccountNum} is {balance} tinybars.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }
}
```

One should note that to construct a payer account, one does need to have access to the necessary private keys to sign the transaction to pay for the balance query (as well as the paying account’s address number).  For the account being queried, only the address number need be known.
