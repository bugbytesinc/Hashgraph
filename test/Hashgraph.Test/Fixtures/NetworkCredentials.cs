using Google.Protobuf;
using Hashgraph.Extensions;
using Microsoft.Extensions.Configuration;
using Proto;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Fixtures
{
    public class NetworkCredentials
    {
        private readonly IConfiguration _configuration;
        private ExchangeRate _exchangeRate = null;
        public string NetworkAddress { get { return _configuration["network:address"]; } }
        public int NetworkPort { get { return getAsInt("network:port"); } }
        public long ServerShard { get { return getAsInt("server:shard"); } }
        public long ServerRealm { get { return getAsInt("server:realm"); } }
        public long ServerNumber { get { return getAsInt("server:number"); } }
        public long AccountShard { get { return getAsInt("account:shard"); } }
        public long AccountRealm { get { return getAsInt("account:realm"); } }
        public long AccountNumber { get { return getAsInt("account:number"); } }
        public string MirrorAddress { get { return _configuration["mirror:address"]; } }
        public int MirrorPort { get { return getAsInt("mirror:port"); } }
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
            long agc = 100;
            if (_exchangeRate == null)
            {
                await using (var client = NewClient())
                {
                    _exchangeRate = (await client.GetExchangeRatesAsync()).Current;
                }
            }
            // This is not necessarily correct, but hopefully stable.
            return ((long)(agu * agc * _exchangeRate.HBarEquivalent)) / (_exchangeRate.USDCentEquivalent);
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
        private void OutputSendingRequest(IMessage message)
        {
            if (Output != null)
            {
                if (message is Proto.Transaction transaction && transaction.BodyBytes != null)
                {
                    var transactionBody = Proto.TransactionBody.Parser.ParseFrom(transaction.BodyBytes);
                    Output.WriteLine($"{DateTime.UtcNow}  TX BODY  {JsonFormatter.Default.Format(transactionBody)}");
                    Output.WriteLine($"{DateTime.UtcNow}  └─ SIG → {JsonFormatter.Default.Format(message)}");
                }
                else if (message is Proto.Query query && TryGetQueryTransaction(query, out Proto.Transaction payment) && payment.BodyBytes != null)
                {
                    if (payment.BodyBytes.IsEmpty)
                    {
                        Output.WriteLine($"{DateTime.UtcNow}  QX ASK → {JsonFormatter.Default.Format(message)}");
                    }
                    else
                    {
                        var transactionBody = Proto.TransactionBody.Parser.ParseFrom(payment.BodyBytes);
                        Output.WriteLine($"{DateTime.UtcNow}  QX PYMT  {JsonFormatter.Default.Format(transactionBody)}");
                        Output.WriteLine($"{DateTime.UtcNow}  └─ QRY → {JsonFormatter.Default.Format(message)}");
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
                    payment = query.ContractGetRecords?.Header?.Payment;
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
                case Query.QueryOneofCase.CryptoGetClaim:
                    payment = query.CryptoGetClaim?.Header?.Payment;
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
            }
            return payment != null;
        }
        [CollectionDefinition(nameof(NetworkCredentials))]
        public class FixtureCollection : ICollectionFixture<NetworkCredentials>
        {
        }
    }
}