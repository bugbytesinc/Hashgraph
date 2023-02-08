using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class UpdateAccountTests
{
    private readonly NetworkCredentials _network;
    public UpdateAccountTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Update Account: Can Update Key")]
    public async Task CanUpdateKey()
    {
        var (publicKey, privateKey) = Generator.KeyPair();
        var updatedKeyPair = Generator.KeyPair();
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = 1,
            Endorsement = publicKey
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(new Endorsement(publicKey), originalInfo.Endorsement);

        var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
        {
            Address = createResult.Address,
            Endorsement = new Endorsement(updatedKeyPair.publicKey),
            Signatory = new Signatory(privateKey, updatedKeyPair.privateKey)
        });
        Assert.Equal(ResponseCode.Success, updateResult.Status);

        var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(new Endorsement(updatedKeyPair.publicKey), updatedInfo.Endorsement);
    }
    [Fact(DisplayName = "Update Account: Can Update Key with Record")]
    public async Task CanUpdateKeyWithRecord()
    {
        var (originalPublicKey, originalPrivateKey) = Generator.KeyPair();
        var (updatedPublicKey, updatedPrivateKey) = Generator.KeyPair();
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountWithRecordAsync(new CreateAccountParams
        {
            InitialBalance = 1,
            Endorsement = originalPublicKey
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(new Endorsement(originalPublicKey), originalInfo.Endorsement);

        var record = await client.UpdateAccountWithRecordAsync(new UpdateAccountParams
        {
            Address = createResult.Address,
            Endorsement = new Endorsement(updatedPublicKey),
            Signatory = new Signatory(originalPrivateKey, updatedPrivateKey)
        });
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(new Endorsement(updatedPublicKey), updatedInfo.Endorsement);
    }
    [Fact(DisplayName = "Update Account: Can Update Memo")]
    public async Task CanUpdateMemo()
    {
        var newMemo = Generator.Memo(20, 40);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var record = await fxAccount.Client.UpdateAccountWithRecordAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            Memo = newMemo,
            Signatory = fxAccount
        });
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Equal(newMemo, info.Memo);
    }
    [Fact(DisplayName = "Update Account: Can Update Auto Association Limit")]
    public async Task CanUpdateAutoAssociationLimit()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var newLimit = Generator.Integer(fxAccount.CreateParams.AutoAssociationLimit + 1, fxAccount.CreateParams.AutoAssociationLimit + 100);
        var record = await fxAccount.Client.UpdateAccountWithRecordAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            AutoAssociationLimit = newLimit,
            Signatory = fxAccount
        });
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Equal(newLimit, info.AutoAssociationLimit);
    }
    [Fact(DisplayName = "NETWORK V0.21.0 UNSUPPORTED: Update Account: Can Update Memo using Alias")]
    public async Task CanUpdateMemoUsingAliasDefect()
    {
        // Updating an account using its alias address has not yet been
        // implemented by the network, although it will accept the transaction.
        var testFailException = (await Assert.ThrowsAsync<TransactionException>(CanUpdateMemoUsingAlias));
        Assert.StartsWith("Unable to update account, status: AccountIdDoesNotExist", testFailException.Message);

        //[Fact(DisplayName = "Update Account: Can Update Memo using Alias")]
        async Task CanUpdateMemoUsingAlias()
        {
            var newMemo = Generator.Memo(20, 40);
            await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
            var record = await fxAccount.Client.UpdateAccountWithRecordAsync(new UpdateAccountParams
            {
                Address = fxAccount.Alias,
                Memo = newMemo,
                Signatory = fxAccount
            });
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
            Assert.Equal(newMemo, info.Memo);
        }
    }
    [Fact(DisplayName = "Update Account: Can Update Memo to Empty")]
    public async Task CanUpdateMemoToEmpty()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var record = await fxAccount.Client.UpdateAccountWithRecordAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            Memo = string.Empty,
            Signatory = fxAccount
        });
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Empty(info.Memo);
    }
    [Fact(DisplayName = "Update Account: Can Update Require Receive Signature")]
    public async Task CanUpdateRequireReceiveSignature()
    {
        var (publicKey, privateKey) = Generator.KeyPair();
        var originalValue = Generator.Integer(0, 1) == 1;
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = 1,
            Endorsement = publicKey,
            RequireReceiveSignature = originalValue,
            Signatory = originalValue ? new Signatory(privateKey) : null   // When True, you need to include signature on create
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(originalValue, originalInfo.ReceiveSignatureRequired);

        var newValue = !originalValue;
        var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
        {
            Address = createResult.Address,
            Signatory = privateKey,
            RequireReceiveSignature = newValue
        });
        Assert.Equal(ResponseCode.Success, updateResult.Status);

        var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(newValue, updatedInfo.ReceiveSignatureRequired);
    }
    [Fact(DisplayName = "Update Account: Can't Update Auto Renew Period to other than 7890000 seconds")]
    public async Task CanUpdateAutoRenewPeriod()
    {
        var (publicKey, privateKey) = Generator.KeyPair();
        var originalValue = TimeSpan.FromSeconds(7890000);
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = 1,
            Endorsement = publicKey,
            AutoRenewPeriod = originalValue
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(originalValue, originalInfo.AutoRenewPeriod);
        Assert.Equal(Address.None, originalInfo.AutoRenewAccount);

        var newValue = originalValue.Add(TimeSpan.FromDays(Generator.Integer(10, 20)));

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = createResult.Address,
                Signatory = privateKey,
                AutoRenewPeriod = newValue
            });
        });
        Assert.Equal(ResponseCode.AutorenewDurationNotInRange, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: AutorenewDurationNotInRange", pex.Message);

        var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(originalValue, updatedInfo.AutoRenewPeriod);
        Assert.Equal(Address.None, updatedInfo.AutoRenewAccount);
    }
    [Fact(DisplayName = "NETWORK V0.22.5 DEFECT: Update Account: Can Update Alias Test Fails")]
    public async Task CanUpdateAliasDefect()
    {
        var testFailException = (await Assert.ThrowsAsync<Xunit.Sdk.EqualException>(CanUpdateAlias));
        Assert.StartsWith("Assert.Equal() Failure", testFailException.Message);

        //[Fact(DisplayName = "Update Account: Can Update Alias")]
        async Task CanUpdateAlias()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var fx = await TestAccount.CreateAsync(_network);

            var originalInfo = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(Alias.None, originalInfo.Alias);
            Assert.Empty(originalInfo.Monikers);

            var updateResult = await fx.Client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = fx.Record.Address,
                Signatory = new Signatory(fx.PrivateKey, privateKey),
                Alias = publicKey
            }); ;
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(new Alias(publicKey), updatedInfo.Alias);
            Assert.Empty(updatedInfo.Monikers);
        }
    }
    [Fact(DisplayName = "NETWORK V0.22.5 DEFECT: Update Account: Can Not Update Alias wihtout Signature Test Fails")]
    public async Task DefectCanNotUpdateAliasWihtoutSignature()
    {
        var testFailException = (await Assert.ThrowsAsync<Xunit.Sdk.ThrowsException>(CanNotUpdateAliasWihtoutSignature));
        Assert.StartsWith("Assert.Throws() Failure", testFailException.Message);

        //[Fact(DisplayName = "Update Account: Can Not Update Alias wihtout Signature")]
        async Task CanNotUpdateAliasWihtoutSignature()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var fx = await TestAccount.CreateAsync(_network);

            var originalInfo = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(Alias.None, originalInfo.Alias);
            Assert.Empty(originalInfo.Monikers);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.UpdateAccountAsync(new UpdateAccountParams
                {
                    Address = fx.Record.Address,
                    Signatory = new Signatory(fx.PrivateKey, privateKey)
                }); ;
            });
            Assert.Equal(ResponseCode.InvalidSignature, pex.Status);
            Assert.StartsWith("Unable to update account, status: InvalidSignature", pex.Message);

            var updatedInfo = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(new Alias(fx.PublicKey), updatedInfo.Alias);
            Assert.Empty(updatedInfo.Monikers);
        }
    }

    [Fact(DisplayName = "NETWORK V0.22.5 DEFECT: Update Account: Can not Update Alias If Already Set Test Fails")]
    public async Task Defect()
    {
        var testFailException = (await Assert.ThrowsAsync<Xunit.Sdk.ThrowsException>(CanNotUpdateAliasIfAlreadySet));
        Assert.StartsWith("Assert.Throws() Failure", testFailException.Message);

        //[Fact(DisplayName = "Update Account: Can not Update Alias If Already Set")]
        async Task CanNotUpdateAliasIfAlreadySet()
        {
            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var fx = await TestAccount.CreateAsync(_network);
            await fx.Client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = fx.Record.Address,
                Signatory = new Signatory(fx.PrivateKey, privateKey1),
                Alias = publicKey1
            }); ;

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.UpdateAccountAsync(new UpdateAccountParams
                {
                    Address = fx.Record.Address,
                    Signatory = new Signatory(fx.PrivateKey, privateKey2),
                    Alias = publicKey2
                }); ;
            });
            Assert.Equal(ResponseCode.AliasIsImmutable, tex.Status);
            Assert.StartsWith("Unable to update account, status: AliasIsImmutable", tex.Message);

            var updatedInfo = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(new Alias(publicKey1), updatedInfo.Alias);
            Assert.Empty(updatedInfo.Monikers);
        }
    }
    [Fact(DisplayName = "NETWORK V0.22.5 DEFECT: Update Account: Can not Update Alias That was Created with PayToAlias Test Fails")]
    public async Task CanNotUpdateAliasThatWasCreatedWithPayToAliasDefect()
    {
        var testFailException = (await Assert.ThrowsAsync<Xunit.Sdk.ThrowsException>(CanNotUpdateAliasThatWasCreatedWithPayToAlias));
        Assert.StartsWith("Assert.Throws() Failure", testFailException.Message);

        //[Fact(DisplayName = "Update Account: Can not Update Alias That was Created with PayToAlias")]
        async Task CanNotUpdateAliasThatWasCreatedWithPayToAlias()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var fx = await TestAliasAccount.CreateAsync(_network);

            var originalInfo = await fx.Client.GetAccountInfoAsync(fx.CreateRecord.Address);
            Assert.Equal(fx.Alias, originalInfo.Alias);
            Assert.Empty(originalInfo.Monikers);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.UpdateAccountAsync(new UpdateAccountParams
                {
                    Address = fx.CreateRecord.Address,
                    Signatory = new Signatory(fx.PrivateKey, privateKey),
                    Alias = publicKey
                }); ;
            });
            Assert.Equal(ResponseCode.AliasIsImmutable, tex.Status);
            Assert.StartsWith("Unable to update account, status: AliasIsImmutable", tex.Message);

            var updatedInfo = await fx.Client.GetAccountInfoAsync(fx.CreateRecord.Address);
            Assert.Equal(fx.Alias, updatedInfo.Alias);
            Assert.Empty(updatedInfo.Monikers);
        }
    }
    [Fact(DisplayName = "NETWORK V0.14.0 UNSUPPORTED: Update Account: Update with Insufficient Funds Returns Required Fee Fails")]
    public async Task UpdateWithInsufficientFundsReturnsRequiredFeeNetwork14Regression()
    {
        var testFailException = (await Assert.ThrowsAsync<TransactionException>(UpdateWithInsufficientFundsReturnsRequiredFee));
        Assert.StartsWith("Unable to update account, status: InsufficientTxFee", testFailException.Message);

        //[Fact(DisplayName = "Update Account: Update with Insufficient Funds Returns Required Fee")]
        async Task UpdateWithInsufficientFundsReturnsRequiredFee()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                Endorsement = publicKey,
                Signatory = privateKey,
                RequireReceiveSignature = true
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.True(originalInfo.ReceiveSignatureRequired);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.UpdateAccountAsync(new UpdateAccountParams
                {
                    Address = createResult.Address,
                    Signatory = privateKey,
                    RequireReceiveSignature = false,
                }, ctx =>
                {
                    ctx.FeeLimit = 1;
                });
            });
            Assert.Equal(ResponseCode.InsufficientTxFee, pex.Status);
            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = createResult.Address,
                Signatory = privateKey,
                RequireReceiveSignature = false
            }, ctx =>
            {
                ctx.FeeLimit = (long)pex.RequiredFee;
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.False(updatedInfo.ReceiveSignatureRequired);
        }
    }
    [Fact(DisplayName = "Update Account: Empty Endorsement is Not Allowed")]
    public async Task EmptyEndorsementIsNotAllowed()
    {
        var (originalPublicKey, originalPrivateKey) = Generator.KeyPair();
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = 10,
            Endorsement = originalPublicKey
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(new Endorsement(originalPublicKey), originalInfo.Endorsement);

        var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = createResult.Address,
                Endorsement = Endorsement.None,
                Signatory = new Signatory(originalPrivateKey)
            });
        });
        Assert.Equal("Endorsement", aoe.ParamName);
        Assert.StartsWith("Endorsement can not be 'None', it must contain at least one key requirement.", aoe.Message);

        var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(originalInfo.Endorsement, updatedInfo.Endorsement);

        var receipt = await client.TransferAsync(createResult.Address, _network.Payer, 5, originalPrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var newBalance = await client.GetAccountBalanceAsync(createResult.Address);
        Assert.Equal(5ul, newBalance);
    }
    [Fact(DisplayName = "Update Account: Nested List of Nested List of Endorsement Allowed")]
    public async Task NestedListEndorsementsIsAllowed()
    {
        var (originalPublicKey, originalPrivateKey) = Generator.KeyPair();
        await using var client = _network.NewClient();
        var createResult = await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = 10,
            Endorsement = originalPublicKey
        });
        Assert.Equal(ResponseCode.Success, createResult.Status);

        var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(new Endorsement(originalPublicKey), originalInfo.Endorsement);

        var nestedEndorsement = new Endorsement(new Endorsement(new Endorsement(new Endorsement(new Endorsement(new Endorsement(originalPublicKey))))));
        var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
        {
            Address = createResult.Address,
            Endorsement = nestedEndorsement,
            Signatory = new Signatory(originalPrivateKey)
        });
        Assert.Equal(ResponseCode.Success, updateResult.Status);

        var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
        Assert.Equal(nestedEndorsement, updatedInfo.Endorsement);

        var receipt = await client.TransferAsync(createResult.Address, _network.Payer, 5, originalPrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var newBalance = await client.GetAccountBalanceAsync(createResult.Address);
        Assert.Equal(5ul, newBalance);
    }
    [Fact(DisplayName = "Update Account: Can Update AutoAssociationLimit")]
    public async Task CanUpdateAutoAssociaitonLimit()
    {
        var newLimit = Generator.Integer(20, 40);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var record = await fxAccount.Client.UpdateAccountWithRecordAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            AutoAssociationLimit = newLimit,
            Signatory = fxAccount
        });
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Equal(newLimit, info.AutoAssociationLimit);
    }
    [Fact(DisplayName = "Update Account: Can Update Auto Association Limit to Zero")]
    public async Task CanUpdateAutoAssociationLimitToZero()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var record = await fxAccount.Client.UpdateAccountWithRecordAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            AutoAssociationLimit = 0,
            Signatory = fxAccount
        });
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Equal(0, info.AutoAssociationLimit);
    }
    [Fact(DisplayName = "Update Account: Can't Update Auto Associate Value to Less Than Zero")]
    public async Task CantUpdateAutoAssociateValueToLessThanZero()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {

            var record = await fxAccount.Client.UpdateAccountWithRecordAsync(new UpdateAccountParams
            {
                Address = fxAccount,
                AutoAssociationLimit = -5,
                Signatory = fxAccount
            });
        });
        Assert.Equal("AutoAssociationLimit", ex.ParamName);
        Assert.StartsWith("The maximum number of auto-associaitons must be nonnegative.", ex.Message);

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Equal(fxAccount.CreateParams.AutoAssociationLimit, info.AutoAssociationLimit);
    }
    [Fact(DisplayName = "Update Account: Can Update Auto Associate Value to Greater Than One Thousand")]
    public async Task CanUpdateAutoAssociateValueToGreatherThanOneThousand()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var limit = Generator.Integer(1001, 10000);
        var record = await fxAccount.Client.UpdateAccountWithRecordAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            AutoAssociationLimit = limit,
            Signatory = fxAccount
        }, ctx =>
        {
            ctx.FeeLimit = ctx.FeeLimit * limit / 100;
        });

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Equal(limit, info.AutoAssociationLimit);
    }
    [Fact(DisplayName = "Update Account: Can Not Schedule Update Account")]
    public async Task CanNotScheduleUpdateAccount()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var newValue = !fxAccount.CreateParams.RequireReceiveSignature;

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAccount.Client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = fxAccount,
                RequireReceiveSignature = newValue,
                Signatory = new Signatory(
                    fxAccount,
                    new PendingParams { PendingPayer = fxPayer }
                )
            });
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
    [Fact(DisplayName = "Update Account: Can Update Multiple Properties at Once")]
    public async Task CanUpdateMultiplePropertiesAtOnce()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxTempate = await TestAccount.CreateAsync(_network);
        var record = await fxAccount.Client.UpdateAccountWithRecordAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            Signatory = new Signatory(fxAccount, fxTempate),
            Endorsement = fxTempate.CreateParams.Endorsement,
            RequireReceiveSignature = fxTempate.CreateParams.RequireReceiveSignature,
            Memo = fxTempate.CreateParams.Memo
        });
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Equal(fxAccount.Record.Address, info.Address);
        Assert.NotNull(info.ContractId);
        Assert.False(info.Deleted);
        Assert.Equal(fxTempate.PublicKey, info.Endorsement);
        Assert.Equal(fxAccount.CreateParams.InitialBalance, info.Balance);
        Assert.Equal(fxTempate.CreateParams.RequireReceiveSignature, info.ReceiveSignatureRequired);
        Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        Assert.Equal(Address.None, info.AutoRenewAccount);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(fxTempate.CreateParams.Memo, info.Memo);
        Assert.Equal(0, info.AssetCount);
        Assert.Equal(fxAccount.CreateParams.AutoAssociationLimit, info.AutoAssociationLimit);
        Assert.Equal(Alias.None, info.Alias);
        Assert.Empty(info.Monikers);
        AssertHg.NotEmpty(info.Ledger);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, info.StakingInfo.PeriodStart);
        Assert.Equal(0, info.StakingInfo.PendingReward);
        Assert.Equal(0, info.StakingInfo.Proxied);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.Equal(0, info.StakingInfo.Node);
    }
    [Fact(DisplayName = "NETWORK V0.21.0 UNSUPPORTED: Update Account: Can Update Key of Alias Account")]
    public async Task CanUpdateKeyOfAliasAccountDefect()
    {
        // Updating an account using its alias address has not yet been
        // implemented by the network, although it will accept the transaction.
        var testFailException = (await Assert.ThrowsAsync<TransactionException>(CanUpdateKeyOfAliasAccount));
        Assert.StartsWith("Unable to update account, status: AccountIdDoesNotExist", testFailException.Message);

        //[Fact(DisplayName = "Update Account: Can Update Key of Alias Account")]
        async Task CanUpdateKeyOfAliasAccount()
        {
            await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
            var (publicKey, privateKey) = Generator.KeyPair();

            var originalInfo = await fxAccount.Client.GetAccountInfoAsync(fxAccount.CreateRecord.Address);
            Assert.Equal(new Endorsement(fxAccount.PublicKey), originalInfo.Endorsement);

            var updateResult = await fxAccount.Client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = fxAccount.Alias,
                Endorsement = new Endorsement(publicKey),
                Signatory = new Signatory(fxAccount.PrivateKey, privateKey)
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await fxAccount.Client.GetAccountInfoAsync(fxAccount.CreateRecord.Address);
            Assert.Equal(new Endorsement(publicKey), updatedInfo.Endorsement);
        }
    }

    [Fact(DisplayName = "Update Account: Protobuf does not contain Alias Update Functionality")]
    public void ProtobufCoesNotContainAliasUpdateFunctionality()
    {
        // This is a marker test as a backup to catch when the functionality
        // for updating an Alias re-appears in the protobuf (it was taken out)
        // When it re-appears, we re-implement the feature, basic tests are
        // already in place for when this happens.
        var type = typeof(Proto.CryptoUpdateTransactionBody);
        var definition = type.GetProperty("Alias");
        Assert.Null(definition);
    }
    [Fact(DisplayName = "Update Account: Can Update Staking Node")]
    public async Task CanUpdateStakingNode()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await fxAccount.Client.UpdateAccountAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            StakedNode = 3,
            Signatory = fxAccount
        });

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.Equal(3, info.StakingInfo.Node);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.Equal(0, info.StakingInfo.Proxied);
    }
    [Fact(DisplayName = "Update Account: Can Update Staking Address")]
    public async Task CanUpdateStakingPrxoyAddress()
    {
        await using var fxProxied = await TestAccount.CreateAsync(_network);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await fxAccount.Client.UpdateAccountAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            ProxyAccount = fxProxied.Record.Address,
            Signatory = fxAccount
        });

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.Equal(0, info.StakingInfo.Node);
        Assert.Equal(fxProxied.Record.Address, info.StakingInfo.Proxy);
        Assert.Equal(0, info.StakingInfo.Proxied);
    }
    [Fact(DisplayName = "Update Account: Can Decline State Reward")]
    public async Task CanDeclineStateReward()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await fxAccount.Client.UpdateAccountAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            DeclineStakeReward = true,
            Signatory = fxAccount
        }); ;

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.NotNull(info.StakingInfo);
        Assert.True(info.StakingInfo.Declined);
        Assert.Equal(0, info.StakingInfo.Node);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.Equal(0, info.StakingInfo.Proxied);
    }
}