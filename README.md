# Hashgraph
.NET Core Client Library for Hedera Hashgraph

## Documentation
We've started working on our documentation, you can see our progress [here](https://bugbytesinc.github.io/Hashgraph/).

__Please Note__:  This library is very new and as we gain experience interacting with the Hedera Network any commit to this project may cause breaking changes.  Please be patient as we gain the necessary experience to provide a best in class library to access the Hedera Network.  

## Example
The ```Client``` object orchestrates the request construction and communication with the hedera network.   It requires a small amount of configuration when created.  At a minimum to retrieve an account balance, the client must be configured with a Gateway and Payer Account.  The ```Gateway``` object represents the internet network address and account for the node processing requests and the Payer ```Account``` represents the account that will sign and pay for the query.  The following code example illustrates retrieving an account balance for an ```Address```:
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
## Installation
```
dotnet add package Hashgraph
```
Hashgraph requires .NET Core Version 3.x

## Contributing
While we are in the process of building the preliminary infrastructure for this project, please direct any feedback, requests or questions to  [Hedera’s Discord](https://discordapp.com/invite/FFb9YFX) channel.

## Cloning
This project references the [Hedera Protobuf](https://github.com/hashgraph/hedera-protobuf)
project as a git submodule (currently the vNext Branch).  It is recommended to include ```--recurse-submodules``` options 
when cloning the repository so that the ```*.proto``` files from the submodule are present
when building the project:
```
$ git clone --recurse-submodules https://github.com/bugbytesinc/Hashgraph.git
```

## Build Status
[![Build Status](https://bugbytes.visualstudio.com/Hashgraph/_apis/build/status/Hashgraph%20Continuous%20Build?branchName=master)](https://bugbytes.visualstudio.com/Hashgraph/_build/latest?definitionId=27&branchName=master)

## Build Requirements
This project relies protobuf support found in .net core 3, 
previous versions of the .net core framework will not work.
(At the time of this writing we are in [preview8](https://dotnet.microsoft.com/download/dotnet-core/3.0))

Visual Studio is not required to build the library, however the project
references the [NSec.Cryptography](https://nsec.rocks/) library, which 
loads the libsodium.dll library which relies upon the VC++ runtime. In
order to execute tests, the [Microsoft Visual C++ Redistributable](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads)
must be installed on the build agent if Visual Studio is not.

## License
Hashgraph is licensed under the [Apache 2.0 license](https://licenses.nuget.org/Apache-2.0).