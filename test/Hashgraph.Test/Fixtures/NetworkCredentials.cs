using Google.Protobuf;
using Hashgraph.Extensions;
using Microsoft.Extensions.Configuration;
using Proto;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Fixtures
{
    public class NetworkCredentials
    {
        private readonly IConfiguration _configuration;
        private ExchangeRate _exchangeRate = null;
        private Address _systemAccountAddress = null;
        private Address _systemDeleteAdminAddress = null;
        private Address _systemUndeleteAdminAddress = null;
        private Address _systemFreezeAdminAddress = null;

        public string NetworkAddress { get { return _configuration["network:address"]; } }
        public int NetworkPort { get { return GetAsInt("network:port"); } }
        public long ServerShard { get { return GetAsInt("server:shard"); } }
        public long ServerRealm { get { return GetAsInt("server:realm"); } }
        public long ServerNumber { get { return GetAsInt("server:number"); } }
        public long AccountShard { get { return GetAsInt("account:shard"); } }
        public long AccountRealm { get { return GetAsInt("account:realm"); } }
        public long AccountNumber { get { return GetAsInt("account:number"); } }
        public string MirrorAddress { get { return _configuration["mirror:address"]; } }
        public int MirrorPort { get { return GetAsInt("mirror:port"); } }
        public ReadOnlyMemory<byte> PrivateKey { get { return Hex.ToBytes(_configuration["account:privateKey"]); } }
        public ReadOnlyMemory<byte> PublicKey { get { return Hex.ToBytes(_configuration["account:publicKey"]); } }
        public Address Payer { get { return new Address(AccountShard, AccountRealm, AccountNumber); } }
        public Signatory Signatory { get { return new Signatory(PrivateKey); } }
        public Gateway Gateway { get { return new Gateway($"{NetworkAddress}:{NetworkPort}", ServerShard, ServerRealm, ServerNumber); } }
        public ITestOutputHelper Output { get; set; }
        public NetworkCredentials()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .AddUserSecrets<NetworkCredentials>(true)
                .Build();
        }
        public Client NewClient()
        {
            return new Client(ctx =>
            {
                ctx.Gateway = Gateway;
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
                ctx.Url = $"{MirrorAddress}:{MirrorPort}";
                ctx.OnSendingRequest = OutputSendingRequest;
            });
        }
        public async Task<long> TinybarsFromGas(double agu)
        {
            // agu = Arbitrary Gas Unit - we really have no idea, some 'relative' unit of work.
            // agc = Arbitrary Gas Constant - Well, we know this is not linear with the exchange rate so we have to keep changing it.
            // Change the agc when the gas price goes up/down whatever.
            long agc = 200;
            if (_exchangeRate == null)
            {
                await using var client = NewClient();
                _exchangeRate = (await client.GetExchangeRatesAsync()).Current;
            }
            // This is not necessarily correct, but hopefully stable.
            return ((long)(agu * agc * _exchangeRate.HBarEquivalent)) / (_exchangeRate.USDCentEquivalent);
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
                    if(transactionBody.ScheduleCreate is not null)
                    {
                        var scheduledTransaction = TransactionBody.Parser.ParseFrom(transactionBody.ScheduleCreate.TransactionBody);
                        Output.WriteLine($"{DateTime.UtcNow}  TX BODY  {JsonFormatter.Default.Format(scheduledTransaction)}");
                        Output.WriteLine($"{DateTime.UtcNow}  ├─ SCH → {JsonFormatter.Default.Format(transactionBody)}");
                        Output.WriteLine($"{DateTime.UtcNow}  └─ SIG → {JsonFormatter.Default.Format(signedTransaction.SigMap)}");
                    }
                    else
                    {
                        Output.WriteLine($"{DateTime.UtcNow}  TX BODY  {JsonFormatter.Default.Format(transactionBody)}");
                        Output.WriteLine($"{DateTime.UtcNow}  └─ SIG → {JsonFormatter.Default.Format(signedTransaction.SigMap)}");
                    }
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