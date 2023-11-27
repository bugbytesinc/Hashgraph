using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Hashgraph.Extensions;
using Hashgraph.Implementation;
using Hashgraph.Mirror;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto.Parameters;
using Proto;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Fixtures;

public class NetworkCredentials
{
    private readonly IConfiguration _configuration;
    private Client _rootClient;
    private MirrorRestClient _mirrorClient;
    private Gateway _gateway;
    private ReadOnlyMemory<byte> _privateKey;
    private Signatory _signatory;
    private Address _systemAccountAddress = null;
    private Address _systemDeleteAdminAddress = null;
    private Address _systemUndeleteAdminAddress = null;
    private Address _systemFreezeAdminAddress = null;
    private ReadOnlyMemory<byte> _ledger = default;
    private AccountData _rootPayer = default;
    private readonly string _mirrorGrpcUrl = default;

    public ITestOutputHelper Output { get; set; }

    public Gateway Gateway => _gateway;
    public Address Payer => _rootPayer.Account;
    public ReadOnlyMemory<byte> PrivateKey => _privateKey;
    public ReadOnlyMemory<byte> PublicKey => _rootPayer.Endorsement.ToBytes();
    public Signatory Signatory => _signatory;
    public ReadOnlyMemory<byte> Ledger => _ledger;
    public MirrorRestClient MirrorRestClient => _mirrorClient;
    public NetworkCredentials()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true)
            .AddEnvironmentVariables()
            .AddUserSecrets<NetworkCredentials>(true)
            .Build();
        var mirrorRestUrl = _configuration["mirrorRestUrl"];
        if (string.IsNullOrWhiteSpace(mirrorRestUrl))
        {
            throw new Exception("Mirror REST URL is missing from configuration [mirrorRestUrl]");
        }
        _mirrorGrpcUrl = _configuration["mirrorGrpcUrl"];
        if (string.IsNullOrWhiteSpace(_mirrorGrpcUrl))
        {
            throw new Exception("Mirror GRPC URL is missing from configuration [mirrorGrpcUrl]");
        }
        var payerPrivateKey = _configuration["payerPrivateKey"];
        if (string.IsNullOrWhiteSpace(payerPrivateKey))
        {
            throw new Exception("Payer Account Private Key is missing from configuration [payerPrivateKey]");
        }
        Task.Run(async () =>
        {
            _mirrorClient = new MirrorRestClient(mirrorRestUrl);
            _gateway = await PickGatewayAsync();
            _privateKey = Hex.ToBytes(payerPrivateKey);
            _signatory = new Signatory(_privateKey);
            _rootPayer = await LookupPayerAsync();
            _rootClient = new Client(channelFactory, ctx =>
            {
                ctx.Gateway = _gateway;
                ctx.Payer = _rootPayer.Account;
                ctx.Signatory = Signatory;
                ctx.RetryCount = 50; // Use a high number, sometimes the test network glitches.
                ctx.RetryDelay = TimeSpan.FromMilliseconds(50); // Use this setting for a while to see if we can trim a few ms off of each test
                ctx.OnSendingRequest = OutputSendingRequest;
                ctx.OnResponseReceived = OutputReceivResponse;
                ctx.AdjustForLocalClockDrift = true; // Build server has clock drift issues
                ctx.FeeLimit = 60_00_000_000; // Testnet is getting pricey.
            });
            var info = await _rootClient.GetAccountInfoAsync(_rootPayer.Account);
            _ledger = info.Ledger;
        }).Wait();
    }

    public Client NewClient()
    {
        return _rootClient.Clone();
    }
    public MirrorGrpcClient NewMirror()
    {
        return new MirrorGrpcClient(ctx =>
        {
            ctx.Uri = new Uri(_mirrorGrpcUrl);
            ctx.OnSendingRequest = OutputSendingRequest;
        });
    }
    private void OutputSendingRequest(IMessage message)
    {
        if (Output != null)
        {
            if (message is Proto.Transaction transaction && transaction.SignedTransactionBytes != null)
            {
                var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(transaction.SignedTransactionBytes);
                var transactionBody = Proto.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);
                Output.WriteLine($"{DateTime.UtcNow}  TX BODY  {JsonFormatter.Default.Format(transactionBody)}");
                Output.WriteLine($"{DateTime.UtcNow}  └─ SIG → {JsonFormatter.Default.Format(signedTransaction.SigMap)}");
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
    private static bool TryGetQueryTransaction(Query query, out Proto.Transaction payment)
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
        _systemAccountAddress ??= await GetSpecialAccount(new Address(0, 0, 50));
        return _systemAccountAddress == Address.None ? null : _systemAccountAddress;
    }
    public async Task<Address> GetGenisisAccountAddress()
    {
        _systemAccountAddress ??= await GetSpecialAccount(new Address(0, 0, 2));
        return _systemAccountAddress == Address.None ? null : _systemAccountAddress;
    }
    public async Task<Address> GetSystemDeleteAdminAddress()
    {
        _systemDeleteAdminAddress ??= await GetSpecialAccount(new Address(0, 0, 59));
        return _systemDeleteAdminAddress == Address.None ? null : _systemDeleteAdminAddress;
    }
    public async Task<Address> GetSystemUndeleteAdminAddress()
    {
        _systemUndeleteAdminAddress ??= await GetSpecialAccount(new Address(0, 0, 60));
        return _systemUndeleteAdminAddress == Address.None ? null : _systemUndeleteAdminAddress;
    }
    public async Task<Address> GetSystemFreezeAdminAddress()
    {
        _systemFreezeAdminAddress ??= await GetSpecialAccount(new Address(0, 0, 58));
        return _systemFreezeAdminAddress == Address.None ? null : _systemFreezeAdminAddress;
    }
    public async Task WaitForTransactionInMirror(TxId transactionID)
    {
        // Need to add more to _mirrorClient about getting a TX by ID
        var record = await _rootClient.GetTransactionRecordAsync(transactionID);
        await WaitForMirrorNodeConsensusTimestamp(record.Concensus.Value);
    }
    public async Task WaitForMirrorNodeConsensusTimestamp(ConsensusTimeStamp timestamp)
    {
        var count = 0;
        var latest = await _mirrorClient.GetLatestConsensusTimestampAsync();
        while (latest < timestamp)
        {
            if (count > 500)
            {
                throw new Exception($"The Mirror node appears to be too far out of sync, gave up waiting for {timestamp}");
            }
            count++;
            await Task.Delay(700);
            latest = await _mirrorClient.GetLatestConsensusTimestampAsync();
        }
    }

    public async Task<TRecord> RetryForKnownNetworkIssuesAsync<TRecord>(Func<Task<TRecord>> callback) where TRecord : TransactionRecord
    {
        try
        {
            while (true)
            {
                try
                {
                    return await callback().ConfigureAwait(false);
                }
                catch (PrecheckException pex) when (pex.Status == ResponseCode.TransactionExpired || pex.Status == ResponseCode.Busy || pex.Status == ResponseCode.RpcError)
                {
                    await SwitchRootGatewayAsync();
                    continue;
                }
            }
        }
        catch (TransactionException ex) when (ex.Message?.StartsWith("The Network Changed the price of Retrieving a Record while attempting to retrieve this record") == true)
        {
            var record = await _rootClient.GetTransactionRecordAsync(ex.TxId) as TRecord;
            if (record is not null)
            {
                return record;
            }
            else
            {
                throw;
            }
        }
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

    private async Task<Gateway> PickGatewayAsync()
    {
        try
        {
            var list = (await _mirrorClient.GetActiveGatewaysAsync(2000)).Keys.ToArray();
            if (list.Length == 0)
            {
                throw new Exception("Unable to find a target gossip node, no gossip endpoints are responding.");
            }
            return list[new Random().Next(0, list.Length)];
        }
        catch (Exception ex)
        {
            throw new Exception("Unable to find a target gossip node.", ex);
        }
    }

    private async Task SwitchRootGatewayAsync()
    {
        try
        {
            var validGateways = (await _mirrorClient.GetActiveGatewaysAsync(2000))
                .Where(g => g.Key != _gateway)
                .OrderBy(g => g.Value)
                .Select(g => g.Key)
                .ToArray();
            if (validGateways.Length > 0)
            {
                Output?.WriteLine($"{DateTime.UtcNow}  Tried switching Root Gateway, but none responded to ping, staying with  {_gateway.ShardNum}.{_gateway.RealmNum}.{_gateway.AccountNum} at {_gateway.Uri}");
            }
            else
            {
                _gateway = validGateways[0];
                _rootClient.Configure(ctx => ctx.Gateway = _gateway);
                Output?.WriteLine($"{DateTime.UtcNow}  Switched Root Client Gateway to {_gateway.ShardNum}.{_gateway.RealmNum}.{_gateway.AccountNum} at {_gateway.Uri}");
            }
        }
        catch (Exception ex)
        {
            Output?.WriteLine($"{DateTime.UtcNow}  Tried to switch main Gateway, but no Gateways are resonding: {ex.Message}");
        }
    }


    private async Task<AccountData> LookupPayerAsync()
    {
        var (keyType, keyParams) = KeyUtils.ParsePrivateKey(_privateKey);

        var endorsement = keyType switch
        {
            KeyType.Ed25519 => new Endorsement(KeyType.Ed25519, ((Ed25519PrivateKeyParameters)keyParams).GeneratePublicKey().GetEncoded()),
            KeyType.ECDSASecp256K1 => new Endorsement(KeyType.ECDSASecp256K1, ToEcdsaSecp256k1PublicKeyParameters((ECPrivateKeyParameters)keyParams)),
            _ => throw new Exception("Invalid private key type.")
        };
        await foreach (var account in _mirrorClient.GetAccountsFromEndorsementAsync(endorsement))
        {
            return account;
        }
        throw new Exception("Unable to find payer account from key.");


        static byte[] ToEcdsaSecp256k1PublicKeyParameters(ECPrivateKeyParameters privateKey)
        {
            return privateKey.Parameters.G.Multiply(privateKey.D).GetEncoded(true);
        }
    }
    GrpcChannel channelFactory(Gateway gateway)
    {
        var defaultMethodConfig = new MethodConfig
        {
            Names = { MethodName.Default },
            RetryPolicy = new RetryPolicy
            {
                MaxAttempts = 50,
                InitialBackoff = TimeSpan.FromSeconds(0.05),
                MaxBackoff = TimeSpan.FromSeconds(0.4),
                BackoffMultiplier = 1.01,
                RetryableStatusCodes = { StatusCode.Unavailable, StatusCode.Unknown, StatusCode.Cancelled, StatusCode.Internal }
            }
        };
        var httpHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromSeconds(10),
            EnableMultipleHttp2Connections = false,
        };
        var options = new GrpcChannelOptions
        {
            HttpHandler = httpHandler,
            MaxRetryBufferSize = null,
            DisposeHttpClient = true,
            ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } }
        };
        return GrpcChannel.ForAddress(gateway.Uri, options);
    }

    [CollectionDefinition(nameof(NetworkCredentials))]
    public class FixtureCollection : ICollectionFixture<NetworkCredentials>
    {
    }
}