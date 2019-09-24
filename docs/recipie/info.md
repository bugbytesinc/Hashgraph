---
title: Get Account Info
layout: default
---

# Get Account Info

In preparation for querying detailed information about an account, the first step is to create a Hashgraph `Client` object.  The Client object orchestrates the request construction and communication with the hedera network. It requires a small amount of configuration when created. At a minimum to retrieve the information, the client must be configured with a `Gateway` and Payer `Account`. The Gateway object represents the internet network address and account for the node processing requests and the Payer Account represents the account that will sign and pay for the query. Note, retreiving information about an account is not free, the network requires a nominal payment of a few _tinybars_ from the paying account to process the request.  After creating and configuring the client object, the `GetAccountInfoAsync` method submits the request to the network and returns an `AccountInfo` object describing the details of the account, including its balance in _tinybars_.  The following code example illustrates a small program performing these actions:

```csharp
class Program
{
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
        var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (48 byte Ed25519 private in hex)
        var queryAccountNo = long.Parse(args[4]);     //   2300 (account 0.0.2300)
        try
        {
            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                ctx.Payer = new Account(0, 0, payerAccountNo, payerPrivateKey);
            });
            var account = new Address(0, 0, queryAccountNo);
            var info = await client.GetAccountInfoAsync(account);
            Console.WriteLine($"Account:               0.0.{info.Address.AccountNum}");
            Console.WriteLine($"Smart Contract ID:     {info.SmartContractId}");
            Console.WriteLine($"Proxy Address:         0.0.{info.Proxy.AccountNum}");
            Console.WriteLine($"Balance:               {info.Balance:#,#} tb");
            Console.WriteLine($"Send Record Thrshld:   {info.SendThresholdCreateRecord:#,#} tb");
            Console.WriteLine($"Rec. Record Thrshld:   {info.ReceiveThresholdCreateRecord:#,#} tb");
            Console.WriteLine($"Receive Sig. Required: {info.ReceiveSignatureRequired}");
            Console.WriteLine($"Auto-Renewal Period:   {info.AutoRenewPeriod}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }
}
```

One should note that to construct a payer account, one does need to have access to the necessary private keys to sign the transaction to pay for the network query request (as well as the paying account’s address number).  For the account being queried, only the address number need be known.
