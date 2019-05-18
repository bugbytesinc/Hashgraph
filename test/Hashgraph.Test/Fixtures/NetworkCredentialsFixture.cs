using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Fixtures
{
    public class NetworkCredentialsFixture
    {
        private readonly IConfiguration _configuration;

        public string NetworkAddress { get { return _configuration["network:address"]; } }
        public int NetworkPort { get { return getAsInt("network:port"); } }
        public long ServerRealm { get { return getAsInt("server:realm"); } }
        public long ServerShard { get { return getAsInt("server:shard"); } }
        public long ServerNumber { get { return getAsInt("server:number"); } }
        public long AccountRealm { get { return getAsInt("account:realm"); } }
        public long AccountShard { get { return getAsInt("account:shard"); } }
        public long AccountNumber { get { return getAsInt("account:number"); } }
        public ReadOnlyMemory<byte> AccountPrivateKey { get { return Hex.ToBytes(_configuration["account:privateKey"]); } }
        public ReadOnlyMemory<byte> AccountPublicKey { get { return Hex.ToBytes(_configuration["account:publicKey"]); } }
        public ITestOutputHelper TestOutput { get; set; }

        public NetworkCredentialsFixture()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .AddUserSecrets<NetworkCredentialsFixture>(true)
                .Build();
        }

        public Account CreateDefaultAccount()
        {
            return new Account(AccountRealm, AccountShard, AccountNumber, AccountPrivateKey);
        }

        public Gateway CreateDefaultGateway()
        {
            return new Gateway($"{NetworkAddress}:{NetworkPort}", ServerRealm, ServerShard, ServerNumber);

        }

        public Client CreateClientWithDefaultConfiguration()
        {
            return new Client(ctx =>
            {
                ctx.Gateway = CreateDefaultGateway();
                ctx.Payer = CreateDefaultAccount();
                ctx.RetryCount = 50; // Use a high number, sometimes the test network glitches.
                ctx.OnSendingRequest = OutputSendingRequest;
                ctx.OnResponseReceived = OutputReceivResponse;
            });
        }
        private int getAsInt(string key)
        {
            var valueAsString = _configuration[key];
            if (int.TryParse(valueAsString, out int value))
            {
                return value;
            }
            throw new InvalidProgramException($"Unable to convert configuration '{key}' of '{valueAsString}' into an integer value.");
        }
        private bool getAsBool(string key)
        {
            var valueAsString = _configuration[key];
            if (bool.TryParse(valueAsString, out bool value))
            {
                return value;
            }
            return false;
        }
        private void OutputSendingRequest(IMessage message)
        {
            if (TestOutput != null)
            {
                if (message is Proto.Transaction transaction && transaction.BodyBytes != null)
                {
                    var transactionBody = Proto.TransactionBody.Parser.ParseFrom(transaction.BodyBytes);
                    TestOutput.WriteLine($"{DateTime.UtcNow}  TX BODY  {JsonFormatter.Default.Format(transactionBody)}");
                    TestOutput.WriteLine($"{DateTime.UtcNow}  └─ SIG → {JsonFormatter.Default.Format(message)}");
                }
                else
                {
                    TestOutput.WriteLine($"{DateTime.UtcNow}  TX     → {JsonFormatter.Default.Format(message)}");
                }
            }
        }

        private void OutputReceivResponse(int tryNo, IMessage message)
        {
            TestOutput?.WriteLine($"{DateTime.UtcNow}  RX:({tryNo:00})  {JsonFormatter.Default.Format(message)}");
        }

        [CollectionDefinition(nameof(NetworkCredentialsFixture))]
        public class FixtureCollection : ICollectionFixture<NetworkCredentialsFixture>
        {
        }
    }
}