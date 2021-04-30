using Google.Protobuf;
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
            Assert.Equal(book.Length, book.ToDictionary(n => n.Id).Count);
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
            Assert.Equal(_network.Gateway, node.Address);
            Assert.Empty(node.IpAddress);
            Assert.Equal(0, node.Port);
            Assert.NotEqual(0, node.CertificateHash.Length);
        }
        [Fact(DisplayName = "Address Book: Can Get Address Book Manually")]
        public async Task CanGetAddressBookManually()
        {
            var client = _network.NewClient();
            var file = await client.GetFileContentAsync(AddressBookExtension.ADDRESS_BOOK_FILE_ADDRESS);
            var book = Proto.NodeAddressBook.Parser.ParseFrom(file.ToArray());
            Assert.NotNull(book);
            var json = JsonFormatter.Default.Format(book);
            _network.Output?.WriteLine(json);
        }
    }
}