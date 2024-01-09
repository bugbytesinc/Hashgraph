namespace Hashgraph.Test.Fixtures;

public class TestAliasAccount : IHasCryptoBalance, IHasTokenBalance, IAsyncDisposable
{
    public ReadOnlyMemory<byte> PublicKey;
    public ReadOnlyMemory<byte> PrivateKey;
    public long InitialTransfer;
    public Alias Alias;
    public TransactionRecord TransactionRecord;
    public CreateAccountRecord CreateRecord;
    public NetworkCredentials Network;
    public Client Client;

    public static async Task<TestAliasAccount> CreateAsync(NetworkCredentials networkCredentials, Action<TestAliasAccount> customize = null)
    {
        var fx = new TestAliasAccount();
        networkCredentials.Output?.WriteLine("STARTING SETUP: Pay to Alias Account Instance");
        (fx.PublicKey, fx.PrivateKey) = Generator.KeyPair();
        fx.Network = networkCredentials;
        fx.Client = networkCredentials.NewClient();
        fx.Alias = new Alias(fx.PublicKey);
        fx.InitialTransfer = Generator.Integer(1_00_000_000, 2_00_000_000);
        customize?.Invoke(fx);
        fx.TransactionRecord = await networkCredentials.RetryForKnownNetworkIssuesAsync(async () =>
        {
            return await fx.Client.TransferWithRecordAsync(fx.Network.Payer, fx.Alias, fx.InitialTransfer, ctx =>
            {
                ctx.Memo = ".NET SDK Test: Creating Test Account on Network";
            });
        });
        var createTransactionId = new TxId(fx.TransactionRecord.Id.Address, fx.TransactionRecord.Id.ValidStartSeconds, fx.TransactionRecord.Id.ValidStartNanos, false, 1);
        Assert.Equal(ResponseCode.Success, fx.TransactionRecord.Status);
        fx.CreateRecord = await fx.Client.GetTransactionRecordAsync(createTransactionId) as CreateAccountRecord;
        Assert.NotNull(fx.CreateRecord);
        networkCredentials.Output?.WriteLine("SETUP COMPLETED: Pay to Alias Account Instance");
        return fx;
    }

    public async ValueTask DisposeAsync()
    {
        Network.Output?.WriteLine("STARTING TEARDOWN: Pay to Alias Account Instance");
        try
        {
            await Client.DeleteAccountAsync(CreateRecord.Address, Network.Payer, PrivateKey, ctx =>
              {
                  ctx.Memo = ".NET SDK Test: Attempting to delete Account from Network (if exists)";
              });
        }
        catch
        {
            // Try to recover any funds if possible(unit tests are getting hBar Hungry)
            try
            {
                var balance = await Client.GetAccountBalanceAsync(CreateRecord.Address);
                if (balance > 500)
                {
                    await Client.TransferAsync(CreateRecord.Address, Network.Payer, (long)balance, PrivateKey);
                }
            }
            catch
            {
                // Nothing else to try, move on.
            }
        }
        await Client.DisposeAsync();
        Network.Output?.WriteLine("TEARDOWN COMPLETED: Pay to Alias Account Instance");
    }

    public async Task<ulong> GetCryptoBalanceAsync()
    {
        return await Client.GetAccountBalanceAsync(CreateRecord.Address);
    }

    public async Task<long?> GetTokenBalanceAsync(Address token)
    {
        return await (await Network.GetMirrorRestClientAsync()).GetAccountTokenBalanceAsync(CreateRecord.Address, token);
    }

    public async Task<TokenHoldingData[]> GetTokenBalancesAsync()
    {
        var list = new List<TokenHoldingData>();
        await foreach (var record in (await Network.GetMirrorRestClientAsync()).GetAccountTokenHoldingsAsync(CreateRecord.Address))
        {
            list.Add(record);
        }
#pragma warning disable CS0618 // Type or member is obsolete
        var balances = await Client.GetAccountBalancesAsync(CreateRecord.Address);
        if (Network.HapiTokenBalanceQueriesEnabled)
        {
            Assert.Equal(balances.Tokens.Count, list.Count);
            foreach (var item in balances.Tokens)
            {
                var match = list.FirstOrDefault(t => t.Token == item.Key);
                Assert.NotNull(match);
                Assert.Equal(item.Value.Balance, (ulong)match.Balance);
            }
        }
        else
        {
            Assert.Empty(balances.Tokens);
        }
        var info = await Client.GetAccountInfoAsync(CreateRecord.Address);
        if (Network.HapiTokenBalanceQueriesEnabled)
        {
            Assert.Equal(info.Tokens.Count, list.Count);
            foreach (var item in info.Tokens)
            {
                var match = list.FirstOrDefault(t => t.Token == item.Token);
                Assert.NotNull(match);
                Assert.Equal(item.Balance, (ulong)match.Balance);
            }
        }
        else
        {
            Assert.Empty(info.Tokens);
        }
#pragma warning restore CS0618 // Type or member is obsolete
        return list.ToArray();
    }

    public static implicit operator Address(TestAliasAccount fxAccount)
    {
        return fxAccount.CreateRecord.Address;
    }

    public static implicit operator Signatory(TestAliasAccount fxAccount)
    {
        return new Signatory(fxAccount.PrivateKey);
    }
}