---
title: Hashgraph - .NET Client Library for Hedera Hashgraph
---
# What is Hashgraph?

Hashgraph provides access to the [Hedera Hashgraph](https://www.hedera.com/) Network for the .NET platform.  It manages the communication details with participating [network nodes](https://docs.hedera.com/guides/mainnet/mainnet-nodes) and provides an efficient set asynchronous interface methods for consumption by .NET programs.

Hashgraph is built with [.NET 5](https://docs.microsoft.com/en-us/dotnet/core/introduction)

## How do I Install It?

Hashgraph is published in [NuGet](https://www.nuget.org/packages/Hashgraph/).  You can install it with your favorite NugGet client, for example from the command line:

```sh
dotnet add package Hashgraph
```

The library references a minimum of dependencies.  It relies on .NET’s native [gRPC](https://docs.microsoft.com/en-us/aspnet/core/grpc/) libraries to access Hedera’s network and utilizes the cryptographic services provided by the [Bouncy Castle Project](http://www.bouncycastle.org/).

## What does 'Hello World' for this Library Look like?

The most simple thing one can ask of the Hedera network is the Balance of an Account.  Here is an example console program:

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

Hashgraph provides access to the Hedera network via the [`Client`](xref:Hashgraph.Client) object.  The [`Client`](xref:Hashgraph.Client) object orchestrates the request construction and communication with the hedera network.   During creation, it requires a small amount of configuration.  At a minimum to retrieve an account balance, the client must be configured with a [`Gateway`](xref:Hashgraph.Gateway).  The [`Gateway`](xref:Hashgraph.Gateway) object represents the internet network address and account for the node processing requests.  The [`Address`](xref:Hashgraph.Address) is the identifier of the account to be queried.

## How do I learn more?

* [Examples](crypto/balance.md):  If you prefer to start with code you can copy then modify, we are working up simple examples for the major ways to interact with the network. So far we have examples for the following:
  * Crypto Transactions
    * [Get Account Balance](crypto/balance.md)
    * [Transfer Crypto](crypto/transfer.md)
    * [Get Account Info](crypto/info.md)
    * [Create New Account](crypto/create.md)
  * File Manipulation 
    * [Get File Content](file/getfilecontent.md)

* [API Documentation](~/obj/temp/apiyml/Hashgraph.yml): We have API Documentation generated from the source code itself.  This is useful if you are looking for a low-level understanding of the moving pieces.  

Our documentation is a work in progress and will be adding to it and improving over time as bandwidth permits.

## Is this project Open Source?

Yes, this is an open source project released under the [Apache 2.0 License](https://github.com/bugbytesinc/Hashgraph/blob/master/LICENSE), the source code can be found at https://github.com/bugbytesinc/hashgraph.