using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Extensions
{
    [Collection(nameof(NetworkCredentials))]
    public class GetAddressBookTests
    {
        private readonly NetworkCredentials _network;
        public GetAddressBookTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Address Book: Can Retrieve Address Book")]
        public async Task CanGetAddressBook()
        {
            var client = _network.NewClient();
            var book = await client.GetAddressBookAsync();
            Assert.NotNull(book);
            Assert.NotEmpty(book);
        }
        [Fact(DisplayName = "Address Book: Can Find Current Gateway RSA Key in Address Book")]
        public async Task CanGetCurrentGatewayRsaFromAddressBook()
        {
            var nodeId = $"{_network.Gateway.ShardNum}.{_network.Gateway.RealmNum}.{_network.Gateway.AccountNum}";
            var client = _network.NewClient();
            var book = await client.GetAddressBookAsync();
            var node = book.FirstOrDefault(n => n.Memo == nodeId);
            Assert.NotNull(node);
            Assert.NotNull(node.RsaPublicKey);
            // NOT IMPLEMENTED YET ON TESTNET
            Assert.Equal(0, node.Id);
            Assert.Empty(node.IpAddress);
            Assert.Equal(0, node.Port);
            Assert.Equal(0, node.CertificateHash.Length);
            //Assert.Equal(_network.Gateway, node.Address);
            Assert.Equal(Address.None, node.Address);
        }
    }
}