namespace Hashgraph.Test.Fixtures;

public class TestAccount : IHasCryptoBalance, IHasTokenBalance, IAsyncDisposable
{
    public ReadOnlyMemory<byte> PublicKey;
    public ReadOnlyMemory<byte> PrivateKey;
    public CreateAccountParams CreateParams;
    public CreateAccountRecord Record;
    public NetworkCredentials Network;
    public Client Client;

    public static async Task<TestAccount> CreateAsync(NetworkCredentials networkCredentials, Action<TestAccount> customize = null)
    {
        var fx = new TestAccount();
        networkCredentials.Output?.WriteLine("STARTING SETUP: Test Account Instance");
        (fx.PublicKey, fx.PrivateKey) = Generator.KeyPair();
        fx.Network = networkCredentials;
        fx.Client = networkCredentials.NewClient();
        fx.CreateParams = new CreateAccountParams
        {
            Endorsement = fx.PublicKey,
            InitialBalance = (ulong)Generator.Integer(10, 20),
            Memo = Generator.Memo(20, 40),
            AutoAssociationLimit = Generator.Integer(5, 10)
        };
        customize?.Invoke(fx);
        fx.Record = await networkCredentials.RetryForKnownNetworkIssuesAsync(async () =>
        {
            return await fx.Client.CreateAccountWithRecordAsync(fx.CreateParams, ctx =>
             {
                 ctx.Memo = ".NET SDK Test: Creating Test Account on Network";
             });
        });
        Assert.Equal(ResponseCode.Success, fx.Record.Status);
        networkCredentials.Output?.WriteLine("SETUP COMPLETED: Test Account Instance");
        return fx;
    }

    public async ValueTask DisposeAsync()
    {
        Network.Output?.WriteLine("STARTING TEARDOWN: Test Account Instance");
        try
        {
            await Client.DeleteAccountAsync(Record.Address, Network.Payer, PrivateKey, ctx =>
              {
                  ctx.Memo = ".NET SDK Test: Attempting to delete Account from Network (if exists)";
              });
        }
        catch
        {
            // Try to recover any funds if possible(unit tests are getting hBar Hungry)
            try
            {
                var balance = await Client.GetAccountBalanceAsync(Record.Address);
                if (balance > 500)
                {
                    await Client.TransferAsync(Record.Address, Network.Payer, (long)balance, PrivateKey);
                }
            }
            catch
            {
                // Nothing else to try, move on.
            }
        }
        await Client.DisposeAsync();
        Network.Output?.WriteLine("TEARDOWN COMPLETED: Test Account Instance");
    }

    public async Task<ulong> GetCryptoBalanceAsync()
    {
        return await Client.GetAccountBalanceAsync(Record.Address);
    }

    public async Task<long?> GetTokenBalanceAsync(Address token)
    {
        var result = await (await Network.GetMirrorRestClientAsync()).GetAccountTokenBalanceAsync(Record.Address, token);
#pragma warning disable CS0618 // Type or member is obsolete
        var balance = await Client.GetAccountTokenBalanceAsync(Record.Address, token);
        if (Network.HapiTokenBalanceQueriesEnabled && result > 0)
        {
            Assert.Equal(result, (long)balance);
        }
        else
        {
            Assert.Equal(0UL, balance);
        }
#pragma warning restore CS0618 // Type or member is obsolete
        return result;
    }

    public async Task<TokenHoldingData[]> GetTokenBalancesAsync()
    {
        var list = new List<TokenHoldingData>();
        await foreach (var record in (await Network.GetMirrorRestClientAsync()).GetAccountTokenHoldingsAsync(Record.Address))
        {
            list.Add(record);
        }
#pragma warning disable CS0618 // Type or member is obsolete
        var balances = await Client.GetAccountBalancesAsync(Record.Address);
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
        var info = await Client.GetAccountInfoAsync(Record.Address);
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

    public static implicit operator Address(TestAccount fxAccount)
    {
        return fxAccount.Record.Address;
    }

    public static implicit operator Signatory(TestAccount fxAccount)
    {
        return new Signatory(fxAccount.PrivateKey);
    }
}