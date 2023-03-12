#pragma warning disable CS0618 // Type or member is obsolete
using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class CreateAccountTests
{
    private readonly NetworkCredentials _network;
    public CreateAccountTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Create Account: Can Create Account")]
    public async Task CanCreateAccountAsync()
    {
        var initialBalance = (ulong)Generator.Integer(10, 200);
        var (publicKey, privateKey) = Generator.KeyPair();
        var client = _network.NewClient();
        var createResult = await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = initialBalance,
            Endorsement = publicKey
        });
        Assert.NotNull(createResult);
        Assert.NotNull(createResult.Address);
        Assert.Equal(_network.Gateway.RealmNum, createResult.Address.RealmNum);
        Assert.Equal(_network.Gateway.ShardNum, createResult.Address.ShardNum);
        Assert.True(createResult.Address.AccountNum > 0);

        var info = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(initialBalance, info.Balance);
        Assert.Equal(createResult.Address.RealmNum, info.Address.RealmNum);
        Assert.Equal(createResult.Address.ShardNum, info.Address.ShardNum);
        Assert.Equal(createResult.Address.AccountNum, info.Address.AccountNum);
        Assert.Empty(info.Tokens);
        Assert.False(info.Deleted);
        Assert.Equal(0, info.ContractNonce);
        Assert.Equal(0, info.AutoAssociationLimit);
        Assert.Equal(Alias.None, info.Alias);
        AssertHg.NotEmpty(info.Ledger);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, info.StakingInfo.PeriodStart);
        Assert.Equal(0, info.StakingInfo.PendingReward);
        Assert.Equal(0, info.StakingInfo.Proxied);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.Equal(0, info.StakingInfo.Node);

        // Move remaining funds back to primary account.
        var from = createResult.Address;
        await client.TransferAsync(from, _network.Payer, (long)initialBalance, privateKey);

        var receipt = await client.DeleteAccountAsync(createResult.Address, _network.Payer, privateKey);
        Assert.NotNull(receipt);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var exception = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await client.GetAccountInfoAsync(createResult.Address);
        });
        Assert.StartsWith("Transaction Failed Pre-Check: AccountDeleted", exception.Message);
    }
    [Fact(DisplayName = "Create Account: Set Signature Required True")]
    public async Task CanSetSignatureRequiredTrue()
    {
        var (publicKey, privateKey) = Generator.KeyPair();
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountWithRecordAsync(new CreateAccountParams
        {
            InitialBalance = 1,
            Endorsement = publicKey,
            Signatory = privateKey,
            RequireReceiveSignature = true
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var info = await client.GetAccountInfoAsync(createResult.Address);
        Assert.True(info.ReceiveSignatureRequired);
    }
    [Fact(DisplayName = "Create Account: Set Signature Required False")]
    public async Task CanSetSignatureRequiredFalse()
    {
        var (publicKey, privateKey) = Generator.KeyPair();
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = 1,
            Endorsement = publicKey,
            RequireReceiveSignature = false
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var info = await client.GetAccountInfoAsync(createResult.Address);
        Assert.False(info.ReceiveSignatureRequired);
    }
    [Fact(DisplayName = "Create Account: Empty Endorsement is Not Allowed")]
    public async Task EmptyEndorsementIsNotAllowed()
    {
        await using var client = _network.NewClient();
        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 10,
                Endorsement = Endorsement.None
            });
        });
        Assert.Equal(ResponseCode.KeyRequired, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: KeyRequired", pex.Message);
    }
    [Fact(DisplayName = "Create Account: Can Set Memo")]
    public async Task CanSetMemo()
    {
        var (publicKey, privateKey) = Generator.KeyPair();
        var memo = Generator.Memo(20);
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountWithRecordAsync(new CreateAccountParams
        {
            InitialBalance = 1,
            Endorsement = publicKey,
            Signatory = privateKey,
            Memo = memo
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var info = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(memo, info.Memo);
    }
    [Fact(DisplayName = "Create Account: Can Set Positive Max Token Association")]
    public async Task CanSetMaxTokenAssociation()
    {
        var (publicKey, privateKey) = Generator.KeyPair();
        var limit = Generator.Integer(20, 200);
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountWithRecordAsync(new CreateAccountParams
        {
            InitialBalance = 1,
            Endorsement = publicKey,
            Signatory = privateKey,
            AutoAssociationLimit = limit
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var info = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(limit, info.AutoAssociationLimit);
    }
    [Fact(DisplayName = "Create Account: Can Set Staking Node")]
    public async Task CanSetStakingNode()
    {
        var (publicKey, privateKey) = Generator.KeyPair();
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountWithRecordAsync(new CreateAccountParams
        {
            InitialBalance = 1,
            Endorsement = publicKey,
            Signatory = privateKey,
            StakedNode = 3,
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var info = await client.GetAccountInfoAsync(createResult.Address);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.Equal(3, info.StakingInfo.Node);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.Equal(0, info.StakingInfo.Proxied);
    }
    [Fact(DisplayName = "Create Account: Can Set Proxy Address")]
    public async Task CanSetProxyAddress()
    {
        await using var fxProxied = await TestAccount.CreateAsync(_network);
        var (publicKey, privateKey) = Generator.KeyPair();
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountWithRecordAsync(new CreateAccountParams
        {
            InitialBalance = 1,
            Endorsement = publicKey,
            Signatory = privateKey,
            ProxyAccount = fxProxied.Record.Address
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var info = await client.GetAccountInfoAsync(createResult.Address);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.Equal(0, info.StakingInfo.Node);
        Assert.Equal(fxProxied.Record.Address, info.StakingInfo.Proxy);
        Assert.Equal(0, info.StakingInfo.Proxied);
    }
    [Fact(DisplayName = "Create Account: Can Decline Staking Reward")]
    public async Task CanDeclineStakingReward()
    {
        var (publicKey, privateKey) = Generator.KeyPair();
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountWithRecordAsync(new CreateAccountParams
        {
            InitialBalance = 1,
            Endorsement = publicKey,
            Signatory = privateKey,
            DeclineStakeReward = true
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var info = await client.GetAccountInfoAsync(createResult.Address);
        Assert.NotNull(info.StakingInfo);
        Assert.True(info.StakingInfo.Declined);
        Assert.Equal(0, info.StakingInfo.Node);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.Equal(0, info.StakingInfo.Proxied);
    }
    [Fact(DisplayName = "Create Account: Can Not Schedule Create Account")]
    public async Task CanScheduleCreateAccount()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            var fxAccount = await TestAccount.CreateAsync(_network, fx =>
            {
                fx.CreateParams.Signatory = new PendingParams
                {
                    PendingPayer = fxPayer
                };
            });

            //var transactionReceipt = await fxPayer.Client.SignPendingTransactionAsync(fxAccount.Record.Pending.Id, fxPayer);
            //var pendingReceipt = await fxPayer.Client.GetReceiptAsync(fxAccount.Record.Pending.TxId);
            //Assert.Equal(ResponseCode.Success, pendingReceipt.Status);

            //var createReceipt = Assert.IsType<CreateAccountReceipt>(pendingReceipt);
            //var account = createReceipt.Address;

            //var info = await fxPayer.Client.GetAccountInfoAsync(account);
            //Assert.Equal(account, info.Address);
            //Assert.NotNull(info.SmartContractId);
            //Assert.False(info.Deleted);
            //Assert.NotNull(info.Proxy);
            //Assert.Equal(Address.None, info.Proxy);
            //Assert.Equal(0, info.ProxiedToAccount);
            //Assert.Equal(fxAccount.CreateParams.Endorsement, info.Endorsement);
            //Assert.Equal(fxAccount.CreateParams.InitialBalance, info.Balance);
            //Assert.Equal(fxAccount.CreateParams.RequireReceiveSignature, info.ReceiveSignatureRequired);
            //Assert.Equal(fxAccount.CreateParams.AutoRenewPeriod.TotalSeconds, info.AutoRenewPeriod.TotalSeconds);
            //Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }

    [Fact(DisplayName = "Create Account: Can Create Account With Duplicate Keys")]
    public async Task CanCreateAccountWithDuplicateKeysAsync()
    {
        var initialBalance = (ulong)Generator.Integer(10, 200);
        var (publicKey, privateKey) = Generator.KeyPair();
        var list = Enumerable.Range(0, 5).Select(_ => new Endorsement(publicKey)).ToArray();
        var requiredCount = (uint)(list.Length - 1);
        var client = _network.NewClient();
        var createResult = await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = initialBalance,
            Endorsement = new Endorsement(requiredCount, list)
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);
        var createdAddress = createResult.Address;

        var info = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(initialBalance, info.Balance);
        Assert.Equal(KeyType.List, info.Endorsement.Type);
        Assert.Equal(requiredCount, info.Endorsement.RequiredCount);
        Assert.Equal(list.Length, info.Endorsement.List.Length);
        foreach (var key in info.Endorsement.List)
        {
            Assert.Equal(list[0], key.PublicKey);
        }

        var createdBalance = await client.GetAccountBalanceAsync(createdAddress);
        Assert.Equal(initialBalance, createdBalance);

        await client.TransferAsync(createdAddress, _network.Payer, (long)initialBalance, privateKey);

        var finalBalance = await client.GetAccountBalanceAsync(createdAddress);
        Assert.Equal(0UL, finalBalance);

        var receipt = await client.DeleteAccountAsync(createResult.Address, _network.Payer, privateKey);
        Assert.NotNull(receipt);
        Assert.Equal(ResponseCode.Success, receipt.Status);
    }

    [Fact(DisplayName = "Create Account: Can Create Account With Same Key")]
    public async Task CanCreateAccountsWithSameKeyAsync()
    {
        var initialBalance = (ulong)Generator.Integer(10, 200);
        var (publicKey, privateKey) = Generator.Secp256k1KeyPair();
        var client = _network.NewClient();
        var createResult1 = await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = initialBalance,
            Endorsement = publicKey
        });
        var createResult2 = await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = initialBalance,
            Endorsement = publicKey
        });
        Assert.NotEqual(createResult1.Address, createResult2.Address);

        var info1 = await client.GetAccountInfoAsync(createResult1.Address);
        var info2 = await client.GetAccountInfoAsync(createResult2.Address);
        Assert.Equal(info1.Balance, info2.Balance);
        Assert.Empty(info1.Tokens);
        Assert.Empty(info2.Tokens);
        Assert.False(info1.Deleted);
        Assert.False(info2.Deleted);
        Assert.Equal(0, info1.ContractNonce);
        Assert.Equal(0, info2.ContractNonce);
        Assert.Equal(0, info1.AutoAssociationLimit);
        Assert.Equal(0, info2.AutoAssociationLimit);
        Assert.Equal(Alias.None, info1.Alias);
        Assert.Equal(Alias.None, info2.Alias);
        Assert.Equal(info1.Alias, info2.Alias);
        // HIP-583 Churn
        //Assert.Empty(info2.Monikers);
        //Assert.Empty(info1.Monikers);
        AssertHg.NotEmpty(info1.Ledger);
        AssertHg.NotEmpty(info2.Ledger);
        Assert.NotNull(info1.StakingInfo);
        Assert.NotNull(info2.StakingInfo);
    }
}