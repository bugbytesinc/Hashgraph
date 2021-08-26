# .NET Client Library for Hedera Hashgraph

Hashgraph provides access to the [Hedera Hashgraph](https://www.hedera.com/) Network for the .NET platform.  It manages the communication details with participating [network nodes](https://docs.hedera.com/guides/mainnet/mainnet-nodes) and provides an efficient set asynchronous interface methods for consumption by .NET programs.

## Documentation

For an [introduction](https://bugbytesinc.github.io/Hashgraph/tutorials/index.html) on how to use this library to connect with the Hedera Network, please visit our documentation [website](https://bugbytesinc.github.io/Hashgraph/).

## Cloning
This project references the [Hedera Protobufs](https://github.com/hashgraph/hedera-protobufs)
project as a git submodule (currently the `develop` Branch).  It is recommended to include ```--recurse-submodules``` options 
when cloning the repository so that the ```*.proto``` files from the submodule are present
when building the project:
```
$ git clone --recurse-submodules https://github.com/bugbytesinc/Hashgraph.git
```

## Contributing
While we are in the process of building the preliminary infrastructure for this project, please direct any feedback, requests or questions to  [Hedera’s Discord](https://discordapp.com/invite/FFb9YFX) channel.

## Build Status

| Main Branch | vNext (Preview Network)
| - | -
| [![Build Status](https://github.com/bugbytesinc/Hashgraph/actions/workflows/testnet.yml/badge.svg)](https://github.com/bugbytesinc/Hashgraph/actions/workflows/testnet.yml) | [![Build Status](https://github.com/bugbytesinc/Hashgraph/actions/workflows/previewnet.yml/badge.svg)](https://github.com/bugbytesinc/Hashgraph/actions/workflows/previewnet.yml)

## Packages

| Nuget
| - 
[![NuGet](https://img.shields.io/nuget/v/hashgraph.svg)](http://www.nuget.org/packages/hashgraph/)


## Build Requirements
This project relies protobuf support found in .net core 5, 
previous versions of the .net core framework will not work.
(At the time of this writing we are in [5.0.103](https://dotnet.microsoft.com/download/dotnet-core/5.0))

Visual Studio is not required to build the library, however the project
references the [NSec.Cryptography](https://nsec.rocks/) library, which 
loads the libsodium.dll library which relies upon the VC++ runtime. In
order to execute tests, the [Microsoft Visual C++ Redistributable](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads)
must be installed on the build agent if Visual Studio is not.

## License
Hashgraph is licensed under the [Apache 2.0 license](https://licenses.nuget.org/Apache-2.0).