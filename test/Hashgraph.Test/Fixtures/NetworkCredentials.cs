using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto.Parameters;

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
    private ConsensusTimeStamp _latestKnownMutatingTimestamp = ConsensusTimeStamp.MinValue;
    private ConsensusTimeStamp _latestKnownMirrorTimestamp = ConsensusTimeStamp.MinValue;
    private bool _hapiTokenBalanceQueriesEnabeled = false;

    public ITestOutputHelper Output { get; set; }
    public Gateway Gateway => _gateway;
    public Address Payer => _rootPayer.Account;
    public ReadOnlyMemory<byte> PrivateKey => _privateKey;
    public ReadOnlyMemory<byte> PublicKey => _rootPayer.Endorsement.ToBytes();
    public Signatory Signatory => _signatory;
    public ReadOnlyMemory<byte> Ledger => _ledger;
    public bool HapiTokenBalanceQueriesEnabled => _hapiTokenBalanceQueriesEnabeled;
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
            _rootClient = new Client(ctx =>
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
                ctx.QueryTip = 2; // Testnet cost query is unreliable.
            });
            var info = await _rootClient.GetAccountInfoAsync(_rootPayer.Account);
            _ledger = info.Ledger;

            _hapiTokenBalanceQueriesEnabeled = await QueryHapiBalanceQueryStatusAsync();

        }).Wait();
    }
    public Client NewClient()
    {
        return _rootClient.Clone();
    }
    public MirrorGrpcClient NewMirrorGrpcClient()
    {
        return new MirrorGrpcClient(ctx =>
        {
            ctx.Uri = new Uri(_mirrorGrpcUrl);
            ctx.OnSendingRequest = OutputSendingRequest;
        });
    }
    public async Task<MirrorRestClient> GetMirrorRestClientAsync()
    {
        await WaitForMirrorConsensusCatchUpAsync();
        return _mirrorClient;
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
                _latestKnownMutatingTimestamp = ConsensusTimeStamp.MinValue;
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
                    // Mutating the payer account balance will not change
                    // the state of an account under test via a Query.
                    if (transactionBody.TransactionID.AccountID.AsAddress() != _rootPayer.Account)
                    {
                        _latestKnownMutatingTimestamp = ConsensusTimeStamp.MinValue;
                    }
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
        if (message is Proto.Response response)
        {
            var record = response.TransactionGetRecord?.TransactionRecord;
            if (record != null)
            {
                _latestKnownMutatingTimestamp = record.ConsensusTimestamp.ToConsensusTimeStamp();
            }
        }
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
    public async Task<Address> GetSystemAccountAddressAsync()
    {
        _systemAccountAddress ??= await GetSpecialAccountAsync(new Address(0, 0, 50));
        return _systemAccountAddress == Address.None ? null : _systemAccountAddress;
    }
    public async Task<Address> GetGenisisAccountAddressAsync()
    {
        _systemAccountAddress ??= await GetSpecialAccountAsync(new Address(0, 0, 2));
        return _systemAccountAddress == Address.None ? null : _systemAccountAddress;
    }
    public async Task<Address> GetSystemDeleteAdminAddressAsync()
    {
        _systemDeleteAdminAddress ??= await GetSpecialAccountAsync(new Address(0, 0, 59));
        return _systemDeleteAdminAddress == Address.None ? null : _systemDeleteAdminAddress;
    }
    public async Task<Address> GetSystemUndeleteAdminAddressAsync()
    {
        _systemUndeleteAdminAddress ??= await GetSpecialAccountAsync(new Address(0, 0, 60));
        return _systemUndeleteAdminAddress == Address.None ? null : _systemUndeleteAdminAddress;
    }
    public async Task<Address> GetSystemFreezeAdminAddressAsync()
    {
        _systemFreezeAdminAddress ??= await GetSpecialAccountAsync(new Address(0, 0, 58));
        return _systemFreezeAdminAddress == Address.None ? null : _systemFreezeAdminAddress;
    }
    private async Task WaitForMirrorConsensusCatchUpAsync()
    {
        if (_latestKnownMutatingTimestamp == ConsensusTimeStamp.MinValue)
        {
            await _rootClient.TransferWithRecordAsync(Payer, Gateway, 1, ctx => ctx.Memo = "Mirror Node HIP-367 Consensus Time Marker");
        }
        if (_latestKnownMirrorTimestamp < _latestKnownMutatingTimestamp)
        {
            Output?.WriteLine($"{DateTime.UtcNow}  WAIT   ⇌ Waiting for Mirror Consensus Timestamp {_latestKnownMutatingTimestamp}");
            var count = 0;
            var httpErrorCount = 0;
            do
            {
                try
                {
                    _latestKnownMirrorTimestamp = await _mirrorClient.GetLatestConsensusTimestampAsync();
                    while (_latestKnownMirrorTimestamp < _latestKnownMutatingTimestamp)
                    {
                        if (count > 500)
                        {
                            throw new Exception($"The Mirror node appears to be too far out of sync, gave up waiting for {_latestKnownMutatingTimestamp}");
                        }
                        count++;
                        await Task.Delay(700);
                        _latestKnownMirrorTimestamp = await _mirrorClient.GetLatestConsensusTimestampAsync();
                    }
                    return;
                }
                catch (HttpRequestException hre)
                {
                    httpErrorCount++;
                    Output?.WriteLine($"{DateTime.UtcNow}  WAIT   ⇌ The Mirror node appears to be struggling {hre.Message}");
                }
            } while (httpErrorCount < 1000);
            throw new Exception($"The Mirror node appears to have gone off to lala land, gave up waiting for {_latestKnownMutatingTimestamp}");
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
    private async Task<Address> GetSpecialAccountAsync(Address address)
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

    private async Task<bool> QueryHapiBalanceQueryStatusAsync()
    {
        await using var fxAccount = await TestAccount.CreateAsync(this);
        await using var fxToken = await TestToken.CreateAsync(this, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var xferAmount = 2 * fxToken.Params.Circulation / 3;
        var receipt = await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxAccount.Record.Address, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        IMessage interceptedMessage = null;
        await fxToken.Client.GetAccountBalanceAsync(fxAccount, ctx => ctx.OnResponseReceived += (int arg1, IMessage message) => interceptedMessage = message);
        var balances = (interceptedMessage as Response)?.CryptogetAccountBalance;
#pragma warning disable CS0612 // Type or member is obsolete
        return balances.TokenBalances?.Count > 0;
#pragma warning restore CS0612 // Type or member is obsolete
    }


    [CollectionDefinition(nameof(NetworkCredentials))]
    public class FixtureCollection : ICollectionFixture<NetworkCredentials>
    {
    }
}