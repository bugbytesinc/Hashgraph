# .NET Client Library for Hedera Hashgraph

For an [introduction](https://bugbytesinc.github.io/Hashgraph/tutorials/index.html) on how to use this library to connect with the Hedera Network, please visit our [website](https://bugbytesinc.github.io/Hashgraph/).

## Cloning
This project references the [Hedera Protobuf](https://github.com/hashgraph/hedera-protobuf)
project as a git submodule (currently the vNext Branch).  It is recommended to include ```--recurse-submodules``` options 
when cloning the repository so that the ```*.proto``` files from the submodule are present
when building the project:
```
$ git clone --recurse-submodules https://github.com/bugbytesinc/Hashgraph.git
```
## Contributing
While we are in the process of building the preliminary infrastructure for this project, please direct any feedback, requests or questions to  [Hedera’s Discord](https://discordapp.com/invite/FFb9YFX) channel.

## Build Status

| Main Branch | .NET 5 Preview Branch
| - | -
| [![Build Status](https://bugbytes.visualstudio.com/Hashgraph/_apis/build/status/Hashgraph%20Continuous%20Build?branchName=master)](https://bugbytes.visualstudio.com/Hashgraph/_build/latest?definitionId=27&branchName=master) | [![Build Status](https://bugbytes.visualstudio.com/Hashgraph/_apis/build/status/Hashgraph%20Continuous%20Build?branchName=net5)](https://bugbytes.visualstudio.com/Hashgraph/_build/latest?definitionId=27&branchName=master)

## Packages

| Nuget
| - 
[![NuGet](https://img.shields.io/nuget/v/hashgraph.svg)](http://www.nuget.org/packages/hashgraph/)


## Build Requirements
This project relies protobuf support found in .net core 3, 
previous versions of the .net core framework will not work.
(At the time of this writing we are in [3.0.100](https://dotnet.microsoft.com/download/dotnet-core/3.0))

Visual Studio is not required to build the library, however the project
references the [NSec.Cryptography](https://nsec.rocks/) library, which 
loads the libsodium.dll library which relies upon the VC++ runtime. In
order to execute tests, the [Microsoft Visual C++ Redistributable](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads)
must be installed on the build agent if Visual Studio is not.

## License
Hashgraph is licensed under the [Apache 2.0 license](https://licenses.nuget.org/Apache-2.0).