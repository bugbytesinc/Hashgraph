﻿namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class CreateAccountByAliasTests
{
    private readonly NetworkCredentials _network;
    public CreateAccountByAliasTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Create Account By Alias: Can Create Account")]
    public async Task CanCreateAccountAsync()
    {
        var initialPayment = 1_00_000_000;
        var (publicKey, privateKey) = Generator.KeyPair();
        var alias = new Alias(publicKey);
        var client = _network.NewClient();
        var receipt = await client.TransferAsync(_network.Payer, alias, initialPayment);
        Assert.NotNull(receipt);

        // If an account was created by the alias, the receipt
        // with the address is a "child receipt" of the transfer
        // receipt and must be explictly asked for.
        var allReceipts = await client.GetAllReceiptsAsync(receipt.Id);
        Assert.Equal(2, allReceipts.Count);
        Assert.Equal(receipt, allReceipts[0]);

        var createReceipt = allReceipts[1] as CreateAccountReceipt;
        Assert.NotNull(createReceipt);
        Assert.NotNull(createReceipt.Address);
        Assert.Equal(_network.Gateway.RealmNum, createReceipt.Address.RealmNum);
        Assert.Equal(_network.Gateway.ShardNum, createReceipt.Address.ShardNum);
        Assert.True(createReceipt.Address.AccountNum > 0);
        Assert.Equal(1, createReceipt.Id.Nonce);

        var createReceiptByTx = await client.GetReceiptAsync(createReceipt.Id) as CreateAccountReceipt;
        Assert.NotNull(createReceiptByTx);
        Assert.NotNull(createReceiptByTx.Address);
        Assert.Equal(_network.Gateway.RealmNum, createReceiptByTx.Address.RealmNum);
        Assert.Equal(_network.Gateway.ShardNum, createReceiptByTx.Address.ShardNum);
        Assert.Equal(createReceipt.Address, createReceiptByTx.Address);
        Assert.Equal(createReceipt.Id, createReceiptByTx.Id);

        Assert.Equal((ulong)initialPayment, await client.GetAccountBalanceAsync(alias));

        var infoFromAccount = await client.GetAccountInfoAsync(createReceipt.Address);
        Assert.Equal(createReceipt.Address, infoFromAccount.Address);
        Assert.Equal(alias, infoFromAccount.Alias);
        // HIP-583 Churn
        //Assert.Empty(infoFromAccount.Monikers);
        Assert.NotNull(infoFromAccount.ContractId);
        Assert.False(infoFromAccount.Deleted);
        Assert.Equal(0, infoFromAccount.ContractNonce);
        Assert.Equal(new Endorsement(publicKey), infoFromAccount.Endorsement);
        Assert.True(infoFromAccount.Balance > 0);
        Assert.False(infoFromAccount.ReceiveSignatureRequired);
        Assert.True(infoFromAccount.AutoRenewPeriod.TotalSeconds > 0);
        Assert.True(infoFromAccount.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, infoFromAccount.AssetCount);
        Assert.Equal(0, infoFromAccount.AutoAssociationLimit);
        Assert.Equal("auto-created account", infoFromAccount.Memo);
        AssertHg.NotEmpty(infoFromAccount.Ledger);
        Assert.NotNull(infoFromAccount.StakingInfo);
        Assert.False(infoFromAccount.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAccount.StakingInfo.PeriodStart);
        Assert.Equal(0, infoFromAccount.StakingInfo.PendingReward);
        Assert.Equal(0, infoFromAccount.StakingInfo.Proxied);
        Assert.Equal(Address.None, infoFromAccount.StakingInfo.Proxy);
        Assert.Equal(0, infoFromAccount.StakingInfo.Node);

        var infoFromAlias = await client.GetAccountInfoAsync(alias);
        Assert.Equal(createReceipt.Address, infoFromAlias.Address);
        Assert.Equal(alias, infoFromAlias.Alias);
        // HIP-583 Churn
        //Assert.Empty(infoFromAlias.Monikers);
        Assert.NotNull(infoFromAlias.ContractId);
        Assert.False(infoFromAlias.Deleted);
        Assert.Equal(0, infoFromAlias.ContractNonce);
        Assert.Equal(new Endorsement(publicKey), infoFromAlias.Endorsement);
        Assert.True(infoFromAlias.Balance > 0);
        Assert.False(infoFromAlias.ReceiveSignatureRequired);
        Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
        Assert.True(infoFromAlias.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, infoFromAlias.AssetCount);
        Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
        Assert.Equal("auto-created account", infoFromAlias.Memo);
        AssertHg.Equal(infoFromAccount.Ledger, infoFromAlias.Ledger);
        Assert.NotNull(infoFromAlias.StakingInfo);
        Assert.False(infoFromAlias.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAlias.StakingInfo.PeriodStart);
        Assert.Equal(0, infoFromAlias.StakingInfo.PendingReward);
        Assert.Equal(0, infoFromAlias.StakingInfo.Proxied);
        Assert.Equal(Address.None, infoFromAlias.StakingInfo.Proxy);
        Assert.Equal(0, infoFromAlias.StakingInfo.Node);
    }

    [Fact(DisplayName = "Create Account By Alias: Can Schedule Create Account")]
    public async Task CanScheduleCreateAccountAsync()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        var initialPayment = 1_00_000_000;
        var (publicKey, privateKey) = Generator.KeyPair();
        var alias = new Alias(publicKey);
        var client = _network.NewClient();
        var receipt = await client.TransferAsync(_network.Payer, alias, initialPayment, new PendingParams { PendingPayer = fxPayer });
        Assert.NotNull(receipt);
        Assert.NotNull(receipt.Pending);

        var signingReceipt = await client.SignPendingTransactionAsync(receipt.Pending.Id, fxPayer);
        Assert.Equal(ResponseCode.Success, signingReceipt.Status);

        // If an account was created by the alias, the receipt
        // with the address is a "child receipt" of the transfer
        // receipt and must be explictly asked for.
        var allReceipts = await client.GetAllReceiptsAsync(receipt.Pending.TxId);
        Assert.Equal(2, allReceipts.Count);
        Assert.Equal(receipt.Pending.TxId, allReceipts[0].Id);

        var createReceipt = allReceipts[1] as CreateAccountReceipt;
        Assert.NotNull(createReceipt);
        Assert.NotNull(createReceipt.Address);
        Assert.Equal(_network.Gateway.RealmNum, createReceipt.Address.RealmNum);
        Assert.Equal(_network.Gateway.ShardNum, createReceipt.Address.ShardNum);
        Assert.True(createReceipt.Address.AccountNum > 0);
        Assert.Equal(1, createReceipt.Id.Nonce);

        var createReceiptByTx = await client.GetReceiptAsync(createReceipt.Id) as CreateAccountReceipt;
        Assert.NotNull(createReceiptByTx);
        Assert.NotNull(createReceiptByTx.Address);
        Assert.Equal(_network.Gateway.RealmNum, createReceiptByTx.Address.RealmNum);
        Assert.Equal(_network.Gateway.ShardNum, createReceiptByTx.Address.ShardNum);
        Assert.Equal(createReceipt.Address, createReceiptByTx.Address);
        Assert.Equal(createReceipt.Id, createReceiptByTx.Id);

        Assert.Equal((ulong)initialPayment, await client.GetAccountBalanceAsync(alias));

        var infoFromAccount = await client.GetAccountInfoAsync(createReceipt.Address);
        Assert.Equal(createReceipt.Address, infoFromAccount.Address);
        Assert.Equal(alias, infoFromAccount.Alias);
        // HIP-583 Churn
        //Assert.Empty(infoFromAccount.Monikers);
        Assert.NotNull(infoFromAccount.ContractId);
        Assert.False(infoFromAccount.Deleted);
        Assert.Equal(0, infoFromAccount.ContractNonce);
        Assert.Equal(new Endorsement(publicKey), infoFromAccount.Endorsement);
        Assert.True(infoFromAccount.Balance > 0);
        Assert.False(infoFromAccount.ReceiveSignatureRequired);
        Assert.True(infoFromAccount.AutoRenewPeriod.TotalSeconds > 0);
        Assert.True(infoFromAccount.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, infoFromAccount.AssetCount);
        Assert.Equal(0, infoFromAccount.AutoAssociationLimit);
        Assert.Equal("auto-created account", infoFromAccount.Memo);
        AssertHg.NotEmpty(infoFromAccount.Ledger);
        Assert.NotNull(infoFromAccount.StakingInfo);
        Assert.False(infoFromAccount.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAccount.StakingInfo.PeriodStart);
        Assert.Equal(0, infoFromAccount.StakingInfo.PendingReward);
        Assert.Equal(0, infoFromAccount.StakingInfo.Proxied);
        Assert.Equal(Address.None, infoFromAccount.StakingInfo.Proxy);
        Assert.Equal(0, infoFromAccount.StakingInfo.Node);

        var infoFromAlias = await client.GetAccountInfoAsync(alias);
        Assert.Equal(createReceipt.Address, infoFromAlias.Address);
        Assert.Equal(alias, infoFromAlias.Alias);
        // HIP-583 Churn
        //Assert.Empty(infoFromAlias.Monikers);
        Assert.NotNull(infoFromAlias.ContractId);
        Assert.False(infoFromAlias.Deleted);
        Assert.Equal(0, infoFromAlias.ContractNonce);
        Assert.Equal(new Endorsement(publicKey), infoFromAlias.Endorsement);
        Assert.True(infoFromAlias.Balance > 0);
        Assert.False(infoFromAlias.ReceiveSignatureRequired);
        Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, infoFromAlias.AutoRenewAccount);
        Assert.True(infoFromAlias.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, infoFromAlias.AssetCount);
        Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
        Assert.Equal("auto-created account", infoFromAlias.Memo);
        AssertHg.Equal(infoFromAccount.Ledger, infoFromAlias.Ledger);
        Assert.NotNull(infoFromAccount.StakingInfo);
        Assert.False(infoFromAccount.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAccount.StakingInfo.PeriodStart);
        Assert.Equal(0, infoFromAccount.StakingInfo.PendingReward);
        Assert.Equal(0, infoFromAccount.StakingInfo.Proxied);
        Assert.Equal(Address.None, infoFromAccount.StakingInfo.Proxy);
        Assert.Equal(0, infoFromAccount.StakingInfo.Node);
    }
    [Fact(DisplayName = "Create Account By Alias: Can Create Account via Ed25519 transfer and get Records")]
    public async Task CanCreateAccountViaEd22519TransferAndGetRecords()
    {
        var initialPayment = 1_00_000_000;
        var (publicKey, privateKey) = Generator.Ed25519KeyPair();
        var alias = new Alias(publicKey);
        var client = _network.NewClient();
        var receipt = await client.TransferAsync(_network.Payer, alias, initialPayment);
        Assert.NotNull(receipt);

        // If an account was created by the alias, the receipt
        // with the address is a "child receipt" of the transfer
        // receipt and must be explictly asked for.
        var allRecords = await client.GetAllTransactionRecordsAsync(receipt.Id);
        Assert.Equal(2, allRecords.Count);
        Assert.Null(allRecords[0].ParentTransactionConcensus);

        var createRecord = allRecords[1] as CreateAccountRecord;
        Assert.NotNull(createRecord);
        Assert.NotNull(createRecord.Address);
        Assert.Equal(_network.Gateway.RealmNum, createRecord.Address.RealmNum);
        Assert.Equal(_network.Gateway.ShardNum, createRecord.Address.ShardNum);
        Assert.True(createRecord.Address.AccountNum > 0);
        Assert.Equal(1, createRecord.Id.Nonce);
        Assert.Equal(Moniker.None, createRecord.Moniker);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        //Assert.Equal(allRecords[0].Concensus, createRecord.ParentTransactionConcensus);
        Assert.Null(createRecord.ParentTransactionConcensus);
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        var createRecordByTx = await client.GetTransactionRecordAsync(createRecord.Id) as CreateAccountRecord;
        Assert.NotNull(createRecordByTx);
        Assert.NotNull(createRecordByTx.Address);
        Assert.Equal(_network.Gateway.RealmNum, createRecordByTx.Address.RealmNum);
        Assert.Equal(_network.Gateway.ShardNum, createRecordByTx.Address.ShardNum);
        Assert.Equal(createRecord.Address, createRecordByTx.Address);
        Assert.Equal(createRecord.Id, createRecordByTx.Id);
        Assert.Equal(Moniker.None, createRecordByTx.Moniker);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        //Assert.Equal(allRecords[0].Concensus, createRecordByTx.ParentTransactionConcensus);
        Assert.Null(createRecordByTx.ParentTransactionConcensus);
        // NETWORK V0.21.0 UNSUPPORTED ^^^^            

        Assert.Equal((ulong)initialPayment, await client.GetAccountBalanceAsync(alias));

        var infoFromAccount = await client.GetAccountInfoAsync(createRecord.Address);
        Assert.Equal(createRecord.Address, infoFromAccount.Address);
        Assert.Equal(alias, infoFromAccount.Alias);
        // HIP-583 Churn
        //Assert.Empty(infoFromAccount.Monikers);
        Assert.NotNull(infoFromAccount.ContractId);
        Assert.False(infoFromAccount.Deleted);
        Assert.Equal(0, infoFromAccount.ContractNonce);
        Assert.Equal(new Endorsement(publicKey), infoFromAccount.Endorsement);
        Assert.True(infoFromAccount.Balance > 0);
        Assert.False(infoFromAccount.ReceiveSignatureRequired);
        Assert.True(infoFromAccount.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, infoFromAccount.AutoRenewAccount);
        Assert.True(infoFromAccount.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, infoFromAccount.AssetCount);
        Assert.Equal(0, infoFromAccount.AutoAssociationLimit);
        Assert.Equal("auto-created account", infoFromAccount.Memo);
        AssertHg.NotEmpty(infoFromAccount.Ledger);
        Assert.NotNull(infoFromAccount.StakingInfo);
        Assert.False(infoFromAccount.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAccount.StakingInfo.PeriodStart);
        Assert.Equal(0, infoFromAccount.StakingInfo.PendingReward);
        Assert.Equal(0, infoFromAccount.StakingInfo.Proxied);
        Assert.Equal(Address.None, infoFromAccount.StakingInfo.Proxy);
        Assert.Equal(0, infoFromAccount.StakingInfo.Node);

        var infoFromAlias = await client.GetAccountInfoAsync(alias);
        Assert.Equal(createRecord.Address, infoFromAlias.Address);
        Assert.Equal(alias, infoFromAlias.Alias);
        // HIP-583 Churn
        //Assert.Empty(infoFromAlias.Monikers);
        Assert.NotNull(infoFromAlias.ContractId);
        Assert.False(infoFromAlias.Deleted);
        Assert.Equal(0, infoFromAlias.ContractNonce);
        Assert.Equal(new Endorsement(publicKey), infoFromAlias.Endorsement);
        Assert.True(infoFromAlias.Balance > 0);
        Assert.False(infoFromAlias.ReceiveSignatureRequired);
        Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, infoFromAccount.AutoRenewAccount);
        Assert.True(infoFromAlias.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, infoFromAlias.AssetCount);
        Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
        Assert.Equal("auto-created account", infoFromAlias.Memo);
        AssertHg.Equal(infoFromAccount.Ledger, infoFromAlias.Ledger);
        Assert.NotNull(infoFromAlias.StakingInfo);
        Assert.False(infoFromAlias.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAlias.StakingInfo.PeriodStart);
        Assert.Equal(0, infoFromAlias.StakingInfo.PendingReward);
        Assert.Equal(0, infoFromAlias.StakingInfo.Proxied);
        Assert.Equal(Address.None, infoFromAlias.StakingInfo.Proxy);
        Assert.Equal(0, infoFromAlias.StakingInfo.Node);
    }
    [Fact(DisplayName = "Create Account By Alias: Can Create Account via Secp256k1 Transfer and get Records")]
    public async Task CanCreateAccountViaSecp256k1TransferAndGetRecords()
    {
        var initialPayment = 1_00_000_000;
        var (publicKey, privateKey) = Generator.Secp256k1KeyPair();
        var alias = new Alias(publicKey);
        var moniker = new Moniker(new Endorsement(publicKey));
        var client = _network.NewClient();
        var receipt = await client.TransferAsync(_network.Payer, alias, initialPayment);
        Assert.NotNull(receipt);

        // If an account was created by the alias, the receipt
        // with the address is a "child receipt" of the transfer
        // receipt and must be explictly asked for.
        var allRecords = await client.GetAllTransactionRecordsAsync(receipt.Id);
        Assert.Equal(2, allRecords.Count);
        Assert.Null(allRecords[0].ParentTransactionConcensus);

        var createRecord = allRecords[1] as CreateAccountRecord;
        Assert.NotNull(createRecord);
        Assert.NotNull(createRecord.Address);
        Assert.Equal(_network.Gateway.RealmNum, createRecord.Address.RealmNum);
        Assert.Equal(_network.Gateway.ShardNum, createRecord.Address.ShardNum);
        Assert.True(createRecord.Address.AccountNum > 0);
        Assert.Equal(1, createRecord.Id.Nonce);
        Assert.Equal(moniker, createRecord.Moniker);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        //Assert.Equal(allRecords[0].Concensus, createRecord.ParentTransactionConcensus);
        Assert.Null(createRecord.ParentTransactionConcensus);
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        var createRecordByTx = await client.GetTransactionRecordAsync(createRecord.Id) as CreateAccountRecord;
        Assert.NotNull(createRecordByTx);
        Assert.NotNull(createRecordByTx.Address);
        Assert.Equal(_network.Gateway.RealmNum, createRecordByTx.Address.RealmNum);
        Assert.Equal(_network.Gateway.ShardNum, createRecordByTx.Address.ShardNum);
        Assert.Equal(createRecord.Address, createRecordByTx.Address);
        Assert.Equal(createRecord.Id, createRecordByTx.Id);
        Assert.Equal(moniker, createRecord.Moniker);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        //Assert.Equal(allRecords[0].Concensus, createRecordByTx.ParentTransactionConcensus);
        Assert.Null(createRecordByTx.ParentTransactionConcensus);
        // NETWORK V0.21.0 UNSUPPORTED ^^^^            

        Assert.Equal((ulong)initialPayment, await client.GetAccountBalanceAsync(alias));

        var infoFromAccount = await client.GetAccountInfoAsync(createRecord.Address);
        Assert.Equal(createRecord.Address, infoFromAccount.Address);
        Assert.Equal(alias, infoFromAccount.Alias);
        // HIP-583 Churn
        //Assert.Empty(infoFromAccount.Monikers);
        Assert.NotNull(infoFromAccount.ContractId);
        Assert.False(infoFromAccount.Deleted);
        Assert.Equal(0, infoFromAccount.ContractNonce);
        Assert.Equal(new Endorsement(publicKey), infoFromAccount.Endorsement);
        Assert.True(infoFromAccount.Balance > 0);
        Assert.False(infoFromAccount.ReceiveSignatureRequired);
        Assert.True(infoFromAccount.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, infoFromAccount.AutoRenewAccount);
        Assert.True(infoFromAccount.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, infoFromAccount.AssetCount);
        Assert.Equal(0, infoFromAccount.AutoAssociationLimit);
        Assert.Equal("auto-created account", infoFromAccount.Memo);
        AssertHg.NotEmpty(infoFromAccount.Ledger);
        Assert.NotNull(infoFromAccount.StakingInfo);
        Assert.False(infoFromAccount.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAccount.StakingInfo.PeriodStart);
        Assert.Equal(0, infoFromAccount.StakingInfo.PendingReward);
        Assert.Equal(0, infoFromAccount.StakingInfo.Proxied);
        Assert.Equal(Address.None, infoFromAccount.StakingInfo.Proxy);
        Assert.Equal(0, infoFromAccount.StakingInfo.Node);

        var infoFromAlias = await client.GetAccountInfoAsync(alias);
        Assert.Equal(createRecord.Address, infoFromAlias.Address);
        Assert.Equal(alias, infoFromAlias.Alias);
        // HIP-583 Churn
        //Assert.Empty(infoFromAlias.Monikers);
        Assert.NotNull(infoFromAlias.ContractId);
        Assert.False(infoFromAlias.Deleted);
        Assert.Equal(0, infoFromAlias.ContractNonce);
        Assert.Equal(new Endorsement(publicKey), infoFromAlias.Endorsement);
        Assert.True(infoFromAlias.Balance > 0);
        Assert.False(infoFromAlias.ReceiveSignatureRequired);
        Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, infoFromAccount.AutoRenewAccount);
        Assert.True(infoFromAlias.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, infoFromAlias.AssetCount);
        Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
        Assert.Equal("auto-created account", infoFromAlias.Memo);
        AssertHg.Equal(infoFromAccount.Ledger, infoFromAlias.Ledger);
        Assert.NotNull(infoFromAlias.StakingInfo);
        Assert.False(infoFromAlias.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAlias.StakingInfo.PeriodStart);
        Assert.Equal(0, infoFromAlias.StakingInfo.PendingReward);
        Assert.Equal(0, infoFromAlias.StakingInfo.Proxied);
        Assert.Equal(Address.None, infoFromAlias.StakingInfo.Proxy);
        Assert.Equal(0, infoFromAlias.StakingInfo.Node);
    }
}