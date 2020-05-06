using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class GetVersionInfoTests
    {
        private readonly NetworkCredentials _network;
        public GetVersionInfoTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }

        [Fact(DisplayName = "NOT IMPLEMENTED YET: Get Version Info: Can Get Info for Network")]
        public async Task CanGetInfoForNetworkAsyncNotImplemented()
        {
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await CanGetInfoForNetworkAsync();
            });
            Assert.Equal(ResponseCode.NotSupported, pex.Status);

            //[Fact(DisplayName = "Get Version Info: Can Get Info for Network")]
            /*public*/
            async Task CanGetInfoForNetworkAsync()
            {
                await using var client = _network.NewClient();
                var info = await client.GetVersionInfoAsync();
                Assert.NotEqual(SemanticVersion.None, info.ApiProtobufVersion);
                Assert.NotEqual(SemanticVersion.None, info.HederaServicesVersion);
            }
        }
        [Fact(DisplayName = "NOT IMPLEMENTED YET: Get Version Info: Getting Version Info without paying signature fails.")]
        public async Task GetInfoWithoutPayingSignatureThrowsExceptionNotImplemented()
        {
            var pex = await Assert.ThrowsAsync<Xunit.Sdk.ThrowsException>(async () =>
            {
                await GetInfoWithoutPayingSignatureThrowsException();
            });

            //[Fact(DisplayName = "Get Version Info: Getting Version Info without paying signature fails.")]
            /*public*/
            async Task GetInfoWithoutPayingSignatureThrowsException()
            {
                await using var client = _network.NewClient();
                client.Configure(ctx => ctx.Signatory = null);
                var account = _network.Payer;
                var ioe = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await client.GetVersionInfoAsync();
                });
                Assert.StartsWith("The Payer's signatory (signing key/callback) has not been configured. This is required for retreiving records and other general network Queries. Please check that", ioe.Message);
            }
        }
    }
}
