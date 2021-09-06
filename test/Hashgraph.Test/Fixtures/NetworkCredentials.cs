using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Proto;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Fixtures
{
    public class NetworkCredentials
    {
        private ulong _roundRobinCount = 0;

        private readonly IConfiguration _configuration;
        private Address _systemAccountAddress = null;
        private Address _systemDeleteAdminAddress = null;
        private Address _systemUndeleteAdminAddress = null;
        private Address _systemFreezeAdminAddress = null;

        public long AccountShard { get { return GetAsInt("account:shard"); } }
        public long AccountRealm { get { return GetAsInt("account:realm"); } }
        public long AccountNumber { get { return GetAsInt("account:number"); } }
        public ReadOnlyMemory<byte> PrivateKey { get { return Hex.ToBytes(_configuration["account:privateKey"]); } }
        public ReadOnlyMemory<byte> PublicKey { get { return Hex.ToBytes(_configuration["account:publicKey"]); } }
        public Address Payer { get { return new Address(AccountShard, AccountRealm, AccountNumber); } }
        public Signatory Signatory { get { return new Signatory(PrivateKey); } }
        public Gateway[] Gateways { get; private init; }
        public string MirrorUrl { get; private init; }
        public ITestOutputHelper Output { get; set; }
        public NetworkCredentials()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .AddUserSecrets<NetworkCredentials>(true)
                .Build();
            // Generally we would not hard-code the list of
            // network codes into a code-base, however, it is
            // difficult with the current configuration layout
            // to configure lists of multiple servers, so we
            // will name the list of nodes "testnet" and
            // "previewnet".  We can always fall back to the 
            // original developer secrets layout for local and
            // specialized configurations.
            var networkName = _configuration["network"]?.ToLower();
            Gateways = networkName switch
            {
                // Utilize all known testnet nodes 
                // in a round-robin fashon.
                "testnet" => new[]
                {
                    new Gateway("34.94.106.61:50211", new Address(0,0,3)),
                    new Gateway("35.237.119.55:50211", new Address(0,0,4)),
                    new Gateway("35.245.27.193:50211", new Address(0,0,5)),
                    new Gateway("34.83.112.116:50211", new Address(0,0,6)),
                },
                // Utilize all known previewnet nodes
                // in a round-robin fashon.
                "previewnet" => new[]
                {
                    new Gateway("35.231.208.148:50211", new Address(0, 0, 3)),
                    new Gateway("35.199.15.177:50211", new Address(0, 0, 4)),
                    new Gateway("35.225.201.195:50211", new Address(0, 0, 5)),
                    new Gateway("35.247.109.135:50211", new Address(0, 0, 6)),
                },
                // Fall back to the original design using
                // a specific gateway configured by network:*
                // and server:* properties.
                _ => new[]
                {
                    new Gateway($"{_configuration["network:address"]}:{GetAsInt("network:port")}", GetAsInt("server:shard"), GetAsInt("server:realm"), GetAsInt("server:number")),
                },
            };
            MirrorUrl = networkName switch
            {
                "testnet" => "hcs.testnet.mirrornode.hedera.com:5600",
                "previewnet" => "hcs.previewnet.mirrornode.hedera.com:5600",
                _ => $"{_configuration["mirror:address"]}:{GetAsInt("mirror:port")}"
            };
        }
        public Client NewClient()
        {
            return new Client(ctx =>
            {
                ctx.Gateway = Gateways[Interlocked.Increment(ref _roundRobinCount) % (ulong)Gateways.Length];
                ctx.Payer = Payer;
                ctx.Signatory = Signatory;
                ctx.RetryCount = 50; // Use a high number, sometimes the test network glitches.
                ctx.OnSendingRequest = OutputSendingRequest;
                ctx.OnResponseReceived = OutputReceivResponse;
                ctx.AdjustForLocalClockDrift = true; // Build server has clock drift issues
                ctx.FeeLimit = 60_00_000_000; // Testnet is getting pricey.
            });
        }
        public MirrorClient NewMirror()
        {
            return new MirrorClient(ctx =>
            {
                ctx.Url = MirrorUrl;
                ctx.OnSendingRequest = OutputSendingRequest;
            });
        }
        private int GetAsInt(string key)
        {
            var valueAsString = _configuration[key];
            if (int.TryParse(valueAsString, out int value))
            {
                return value;
            }
            throw new InvalidProgramException($"Unable to convert configuration '{key}' of '{valueAsString}' into an integer value.");
        }
        private void OutputSendingRequest(IMessage message)
        {
            if (Output != null)
            {
                if (message is Proto.Transaction transaction && transaction.SignedTransactionBytes != null)
                {
                    var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(transaction.SignedTransactionBytes);
                    var transactionBody = Proto.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);
                    //if(transactionBody.ScheduleCreate is not null)
                    //{
                    //    var scheduledTransaction = transactionBody.ScheduleCreate.ScheduledTransactionBody;
                    //    Output.WriteLine($"{DateTime.UtcNow}  TX BODY  {JsonFormatter.Default.Format(scheduledTransaction)}");
                    //    Output.WriteLine($"{DateTime.UtcNow}  ├─ SCH → {JsonFormatter.Default.Format(transactionBody)}");
                    //    Output.WriteLine($"{DateTime.UtcNow}  └─ SIG → {JsonFormatter.Default.Format(signedTransaction.SigMap)}");
                    //}
                    //else
                    //{
                    Output.WriteLine($"{DateTime.UtcNow}  TX BODY  {JsonFormatter.Default.Format(transactionBody)}");
                    Output.WriteLine($"{DateTime.UtcNow}  └─ SIG → {JsonFormatter.Default.Format(signedTransaction.SigMap)}");
                    //}
                }
                else if (message is Proto.Query query && TryGetQueryTransaction(query, out Proto.Transaction payment) && payment.SignedTransactionBytes != null)
                {
                    var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(payment.SignedTransactionBytes);
                    if (signedTransaction.BodyBytes.IsEmpty)
                    {
                        Output.WriteLine($"{DateTime.UtcNow}  QX ASK → {JsonFormatter.Default.Format(message)}");
                    }
                    else
                    {
                        var transactionBody = Proto.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);
                        Output.WriteLine($"{DateTime.UtcNow}  QX PYMT  {JsonFormatter.Default.Format(transactionBody)}");
                        Output.WriteLine($"{DateTime.UtcNow}  ├─ SIG → {JsonFormatter.Default.Format(signedTransaction.SigMap)}");
                        Output.WriteLine($"{DateTime.UtcNow}  └─ QRY → {JsonFormatter.Default.Format(query)}");
                    }
                }
                else if (message is Com.Hedera.Mirror.Api.Proto.ConsensusTopicQuery)
                {
                    Output.WriteLine($"{DateTime.UtcNow}  MR-QRY → {JsonFormatter.Default.Format(message)}");
                }
                else
                {
                    Output.WriteLine($"{DateTime.UtcNow}  TX     → {JsonFormatter.Default.Format(message)}");
                }
            }
        }
        private void OutputReceivResponse(int tryNo, IMessage message)
        {
            Output?.WriteLine($"{DateTime.UtcNow}  RX:({tryNo:00})  {JsonFormatter.Default.Format(message)}");
        }
        private bool TryGetQueryTransaction(Query query, out Transaction payment)
        {
            payment = null;
            switch (query.QueryCase)
            {
                case Query.QueryOneofCase.GetByKey:
                    payment = query.GetByKey?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.GetBySolidityID:
                    payment = query.GetBySolidityID?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.ContractCallLocal:
                    payment = query.ContractCallLocal?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.ContractGetInfo:
                    payment = query.ContractGetInfo?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.ContractGetBytecode:
                    payment = query.ContractGetBytecode?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.ContractGetRecords:
#pragma warning disable CS0612 // Type or member is obsolete
                    payment = query.ContractGetRecords?.Header?.Payment;
#pragma warning restore CS0612 // Type or member is obsolete
                    break;
                case Query.QueryOneofCase.CryptogetAccountBalance:
                    payment = query.CryptogetAccountBalance?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.CryptoGetAccountRecords:
                    payment = query.CryptoGetAccountRecords?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.CryptoGetInfo:
                    payment = query.CryptoGetInfo?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.CryptoGetLiveHash:
                    payment = query.CryptoGetLiveHash?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.CryptoGetProxyStakers:
                    payment = query.CryptoGetProxyStakers?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.FileGetContents:
                    payment = query.FileGetContents?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.FileGetInfo:
                    payment = query.FileGetInfo?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.TransactionGetReceipt:
                    payment = query.TransactionGetReceipt?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.TransactionGetRecord:
                    payment = query.TransactionGetRecord?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.TransactionGetFastRecord:
                    payment = query.TransactionGetFastRecord?.Header?.Payment;
                    break;
                case Query.QueryOneofCase.ScheduleGetInfo:
                    payment = query.ScheduleGetInfo?.Header?.Payment;
                    break;
            }
            return payment != null;
        }
        public async Task<Address> GetSystemAccountAddress()
        {
            if (_systemAccountAddress is null)
            {
                _systemAccountAddress = await GetSpecialAccount(new Address(0, 0, 50));
            }
            return _systemAccountAddress == Address.None ? null : _systemAccountAddress;
        }
        public async Task<Address> GetSystemDeleteAdminAddress()
        {
            if (_systemDeleteAdminAddress is null)
            {
                _systemDeleteAdminAddress = await GetSpecialAccount(new Address(0, 0, 59));
            }
            return _systemDeleteAdminAddress == Address.None ? null : _systemDeleteAdminAddress;
        }
        public async Task<Address> GetSystemUndeleteAdminAddress()
        {
            if (_systemUndeleteAdminAddress is null)
            {
                _systemUndeleteAdminAddress = await GetSpecialAccount(new Address(0, 0, 60));
            }
            return _systemUndeleteAdminAddress == Address.None ? null : _systemUndeleteAdminAddress;
        }
        public async Task<Address> GetSystemFreezeAdminAddress()
        {
            if (_systemFreezeAdminAddress is null)
            {
                _systemFreezeAdminAddress = await GetSpecialAccount(new Address(0, 0, 58));
            }
            return _systemFreezeAdminAddress == Address.None ? null : _systemFreezeAdminAddress;
        }
        private async Task<Address> GetSpecialAccount(Address address)
        {
            await using var client = NewClient();
            try
            {
                if (await client.GetAccountBalanceAsync(address) < 75_00_000_000)
                {
                    await client.TransferAsync(Payer, address, 150_00_000_000);
                }
                await client.GetAccountInfoAsync(address, ctx => ctx.Payer = address);
            }
            catch (PrecheckException pex) when (pex.Status == ResponseCode.InvalidSignature)
            {
                // We are not configured with the genisis key and have no access to the special account
                return Address.None;
            }
            return address;
        }

        [CollectionDefinition(nameof(NetworkCredentials))]
        public class FixtureCollection : ICollectionFixture<NetworkCredentials>
        {
        }
    }
}